using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;
using Serilog;

namespace Pushpay.DynamoDbProvisioner
{
	public class Provisioner
	{
		readonly string _prefix;
		readonly Assembly[] _modelAssemblies;
		readonly IAmazonDynamoDB _db;
		readonly List<string> _tablesToRemove = new List<string>();
		readonly CreateTableRequestBuilder _createTableRequestBuilder = new CreateTableRequestBuilder();
		readonly DynamoTimeToLiveRequestBuilder _ttlRequestBuilder = new DynamoTimeToLiveRequestBuilder();

		static readonly SemaphoreSlim SemaphoreSlim = new SemaphoreSlim(1, 1);
		bool _setup;

		static bool _firstTimeCleanupPerformed;
		readonly ILogger _logger = Log.ForContext<Provisioner>();
		readonly TimeSpan _maxTimeToWaitForDynamoServiceAvailability = TimeSpan.FromMinutes(2);
		readonly int _maxSecondsToWaitForTableStatusToChange = 60;

		public IAmazonDynamoDB GetDb()
		{
			return _db;
		}

		public IDynamoDBContext GetContext()
		{
			return new DynamoDBContext(_db, new DynamoDBContextConfig {
				TableNamePrefix = _prefix
			});
		}

		public Provisioner(IAmazonDynamoDB db, string prefix, params Assembly[] modelAssemblies)
		{
			_db = db;
			_prefix = prefix;
			_modelAssemblies = modelAssemblies;
		}

		public async Task Setup(bool performFirstTimeTestTableCleanup = false)
		{
			await SemaphoreSlim.WaitAsync();

			try {
				if (_setup) return;

				_logger.Information("Starting provisioning of tables for Dynamo service at {DynamoUrl}", _db.Config.ServiceURL);

				await WaitForServiceAvailability();

				if (performFirstTimeTestTableCleanup && !_firstTimeCleanupPerformed) {
					await RemoveAllTestTables();
					_firstTimeCleanupPerformed = true;
				}

				try {
					foreach (var assembly in _modelAssemblies) {
						await CreateTablesFromAssembly(assembly);
					}

					_setup = true;
				} catch (HttpRequestException ex) when (IsCannotConnectError(ex)) {
					throw new Exception(@"It does not look like the local Dynamo server is running.

To run local dynamo (and other AWS services) you can use localstack:

> docker run -p 8080:8080/tcp -p 4567-4583:4567-4583/tcp --restart always localstack/localstack

More details on using local stack can be found here:

https://github.com/localstack/localstack
");
				} catch (Exception ex) {
					throw new Exception($"Failed provisioning of tables for Dynamo service at {_db.Config.ServiceURL}", ex);
				}
			} finally {
				SemaphoreSlim.Release();
			}
		}

		async Task WaitForServiceAvailability()
		{
			_logger.Information("Starting to wait for Dynamo service availability at {ServiceUrl}", _db.Config.ServiceURL);

			var watch = Stopwatch.StartNew();
			while (watch.Elapsed < _maxTimeToWaitForDynamoServiceAvailability) {
				try {
					var cts = new CancellationTokenSource();
					cts.CancelAfter(1000);
					await _db.ListTablesAsync(cts.Token);
					break;
				} catch (Exception e) {
					_logger.Information("Dynamo service not yet available, delaying 1 second then trying again for Dynamo service at {ServiceUrl}. {Message}", _db.Config.ServiceURL, e.Message);
					await Task.Delay(TimeSpan.FromSeconds(1));
				}
			}
		}

		async Task RemoveAllTestTables()
		{
			try {
				string lastEvaluatedTableName = null;
				while (true) {
					var tables = await _db.ListTablesAsync(new ListTablesRequest {ExclusiveStartTableName = lastEvaluatedTableName});
					lastEvaluatedTableName = tables.LastEvaluatedTableName;
					var uniqueTableNames = new HashSet<string>(tables.TableNames);
					await Task.WhenAll(uniqueTableNames.Select(DeleteTable));
					if (lastEvaluatedTableName == null) break;
				}
			} catch (Exception ex) {
				throw new Exception("Failed to remove all test tables for ServiceUrl", ex) {
					Data = {
						["ServiceUrl"] = _db.Config.ServiceURL
					}
				};
			}
		}

		async Task DeleteTable(string table)
		{
			if (table.StartsWith(_prefix)) return;
			Guid g;
			if (Guid.TryParse(table.Substring(0, table.IndexOf("_", StringComparison.Ordinal)), out g)) {
				// looks like a table for a different test run, lets remove it
				try {
					_logger.Information("Deleting table {TableName}", table);
					await _db.DeleteTableAsync(new DeleteTableRequest {
						TableName = table
					});
					await WaitTillTableIsDeleted(table);
				} catch (ResourceNotFoundException) {
					// swallow instances where the table no longer exists
				}
			}
		}

		public async Task Cleanup()
		{
			await Task.WhenAll(_tablesToRemove.Select(DeleteTableByName));
		}

		async Task DeleteTableByName(string tableName)
		{
			var tables = await _db.ListTablesAsync();
			if (!tables.TableNames.Contains(tableName)) return;
			
			await _db.DeleteTableAsync(new DeleteTableRequest(tableName));
			Console.WriteLine($"Deleted table {tableName}");
		}

		public async Task CreateTablesFromAssembly(Assembly assembly)
		{
			var tableTypes = assembly.GetTypes().Where(x => x.GetTypeInfo().GetCustomAttribute<DynamoDBTableAttribute>(false) != null);
			await Task.WhenAll(tableTypes.Select(CreateTable));
		}

		public async Task CreateTable<TTable>()
		{
			await CreateTable(typeof(TTable));
		}

		public async Task<int> RemoveAllExistingTables()
		{
			string lastEvaluatedTableName = null;

			int removedTables = 0;

			while (true) {
				var tables = await _db.ListTablesAsync(new ListTablesRequest {
					Limit = 100,
					ExclusiveStartTableName = lastEvaluatedTableName
				});

				var removeTablesMatchingPrefixTasks = tables.TableNames.Where(x => x.StartsWith(_prefix)).Select(DeleteTableByName).ToArray();

				if (removeTablesMatchingPrefixTasks.Length > 0) {
					await Task.WhenAll(removeTablesMatchingPrefixTasks);
				}

				removedTables += removeTablesMatchingPrefixTasks.Length;

				lastEvaluatedTableName = tables.LastEvaluatedTableName;

				if (lastEvaluatedTableName == null) {
					break;
				}
			}

			return removedTables;
		}

		async Task WaitTillTableIsActive(string tableName)
		{
			Stopwatch watch = Stopwatch.StartNew();

			while (watch.Elapsed.TotalSeconds < _maxSecondsToWaitForTableStatusToChange) {
				try {
					var result = await _db.DescribeTableAsync(new DescribeTableRequest {
						TableName = tableName
					});
					if (result.Table.TableStatus == TableStatus.ACTIVE) {
						break;
					}
				} catch (ResourceNotFoundException) {
					break;
				}

				await Task.Delay(10); // keep polling every 10ms until the table has left the in process state
			}
		}

		async Task WaitTillTableIsDeleted(string tableName)
		{
			Stopwatch watch = Stopwatch.StartNew();

			while (watch.Elapsed.TotalSeconds < _maxSecondsToWaitForTableStatusToChange) {
				try {
					var result = await _db.DescribeTableAsync(new DescribeTableRequest {
						TableName = tableName
					});
					if (result.Table.TableStatus != TableStatus.DELETING) {
						break;
					}
				} catch (ResourceNotFoundException) {
					break;
				}

				await Task.Delay(10); // keep polling every 10ms until the table has left the in process state
			}
		}

		async Task<bool> TableExistsAlready(string tableName)
		{
			try {
				var result = await _db.DescribeTableAsync(new DescribeTableRequest {
					TableName = tableName
				});
				return true;
			} catch (ResourceNotFoundException) {
				return false;
			}
		}

		async Task CreateTable(Type type)
		{
			var createTableRequest = _createTableRequestBuilder.BuildFrom(type, _prefix);

			if(await TableExistsAlready(createTableRequest.TableName)){
				_logger.Information("Table {TableName} exists already, skipping provisioning", createTableRequest.TableName);
				return;
			}

			_logger.Information("Deleting table {TableName}", createTableRequest.TableName);

			try {
				await _db.DeleteTableAsync(createTableRequest.TableName);
				await WaitTillTableIsDeleted(createTableRequest.TableName);
			} catch (ResourceNotFoundException) { }

			_logger.Information("Creating table {TableName}", createTableRequest.TableName);

			try {
				await _db.CreateTableAsync(createTableRequest);
				await WaitTillTableIsActive(createTableRequest.TableName);
			} catch (ResourceInUseException rEX) {
				_logger.Warning(rEX, "Failed to create table {TableName}", createTableRequest.TableName);
			}catch (Exception ex) {
				var wrappedEx = new Exception("Failed to create table", ex) {
					Data = {
						["TableName"] = createTableRequest.TableName
					}
				};
				wrappedEx.Data["request"] = JsonConvert.SerializeObject(createTableRequest);
				throw wrappedEx;
			}

			var updateTtlRequest = _ttlRequestBuilder.BuildFrom(type, _prefix);

			if (updateTtlRequest != null) {
				try {
					await _db.UpdateTimeToLiveAsync(updateTtlRequest);
					await WaitTillTableIsActive(createTableRequest.TableName);
				} catch (Exception ex) {
					if (ex.Message == "An unknown operation was requested.") {
						// DynamoDB local does not support TTL so we just ignore these failures.
					} else {
						ex.Data["request"] = JsonConvert.SerializeObject(updateTtlRequest);
						throw;
					}
				}
			}


			if (_tablesToRemove.Contains(createTableRequest.TableName)) {
				throw new Exception("Table cannot be added more than once to the list of tables to be removed.") {
					Data = {
						["TableName"] = createTableRequest.TableName
					}
				};
			}
			_tablesToRemove.Add(createTableRequest.TableName);

			Console.WriteLine($"Added table {createTableRequest.TableName}");
		}

		bool IsCannotConnectError(HttpRequestException ex)
		{
			return ex.InnerException?.Message == "Couldn't connect to server";
		}
	}
}
