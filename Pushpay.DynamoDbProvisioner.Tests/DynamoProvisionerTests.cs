using System.Reflection;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PowerAssert;
using Xunit;
using Pushpay.DynamoDbProvisioner.Tests.Helper;

namespace Pushpay.DynamoDbProvisioner.Tests
{
	public class DynamoProvisionerTests
	{
		readonly Provisioner _provisioner;

		public DynamoProvisionerTests()
		{
			_provisioner = new Provisioner(CreateLocalStackDb(), "unittest_");
		}
		
		static IAmazonDynamoDB CreateLocalStackDb()
		{
			return new AmazonDynamoDBClient("test", "test", new AmazonDynamoDBConfig {
				ServiceURL = "http://localhost:4569/",
				UseHttp = true
			});
		}

		[Fact]
		public async Task ProvisionAndDestroyAllTablesForAssembly()
		{
			await _provisioner.CreateTablesFromAssembly(typeof(DynamoProvisionerTests).GetTypeInfo().Assembly);

			await _provisioner.Cleanup();
		}

		[Fact]
		public async Task WriteThenReadFromProvisionedTable()
		{
			await _provisioner.CreateTable<SimplePerson>();

			var context = _provisioner.GetContext();

			var person = new SimplePerson {
				Age = 22,
				CatsOwned = 1,
				FirstName = "Daniel",
				LastName = "Datum"
			};

			await context.SaveAsync(person);

			var fromDb = await context.LoadAsync<SimplePerson>("Datum", "Daniel");

			using (var poly = PAssert.Poly()) {
				poly.IsTrue(() => fromDb.FirstName == person.FirstName);
				poly.IsTrue(() => fromDb.LastName == person.LastName);
				poly.IsTrue(() => fromDb.Age == person.Age);
				poly.IsTrue(() => fromDb.CatsOwned == person.CatsOwned);
			}

			await _provisioner.Cleanup();
		}

		[Fact]
		public async Task ProvisionTablesEnablesTtlForTables()
		{
			await _provisioner.CreateTable<TableWithTtlProperty>();

			var db = _provisioner.GetDb();

			try {
				var result = await db.DescribeTimeToLiveAsync(new DescribeTimeToLiveRequest {
					TableName = "unittest_" + nameof(TableWithTtlProperty)
				});

				PAssert.IsTrue(() => result.TimeToLiveDescription.AttributeName == "Expiry");
				PAssert.IsTrue(() => result.TimeToLiveDescription.TimeToLiveStatus.In(TimeToLiveStatus.ENABLED, TimeToLiveStatus.ENABLING));
			} catch (AmazonDynamoDBException ex) when (ex.Message == "An unknown operation was requested.") {
				// swallow not-supported exceptions from DynamoDB local for now until support is rolled out
				// for this feature
			}
		}

		[Fact]
		public async Task RemoveAllExistingTables()
		{
			await _provisioner.CreateTablesFromAssembly(typeof(DynamoProvisionerTests).GetTypeInfo().Assembly);

			var count = await _provisioner.RemoveAllExistingTables();

			PAssert.IsTrue(() => count > 0);
		}
	}
}
