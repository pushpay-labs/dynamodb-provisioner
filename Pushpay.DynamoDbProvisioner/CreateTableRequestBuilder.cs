using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Pushpay.DynamoDbProvisioner.Infrastructure;

namespace Pushpay.DynamoDbProvisioner
{
	internal class CreateTableRequestBuilder
	{
		// there is a max with local dynamo of 80,000 capacity units per account - so lets
		// set the limit to 100 r/w per table so we can have plenty of tables
		const int ReadCapacityUnits = 100;
		const int WriteCapacityUnits = 100;

		public CreateTableRequest BuildFrom(Type tableType, string tablePrefix)
		{
			var createTableRequest = new CreateTableRequest {
				AttributeDefinitions = new List<AttributeDefinition>(),
				KeySchema = new List<KeySchemaElement>(),
				GlobalSecondaryIndexes = new List<GlobalSecondaryIndex>(),
				LocalSecondaryIndexes = new List<LocalSecondaryIndex>(),
				ProvisionedThroughput = new ProvisionedThroughput {
					ReadCapacityUnits = ReadCapacityUnits,
					WriteCapacityUnits = WriteCapacityUnits
				}
			};

			var table = tableType.GetTypeInfo().GetCustomAttribute<DynamoDBTableAttribute>();

			createTableRequest.TableName = tablePrefix + table.TableName;

			var properties = tableType.GetProperties();

			ProcessKeys(properties, createTableRequest);

			ProcessGlobalSecondaryIndexHashKeys(tableType, properties, createTableRequest);

			ProcessGlobalSecondaryIndexRangeKeys(tableType, properties, createTableRequest);

			ProcessLocalSecondaryIndexes(tableType, properties, createTableRequest);

			return createTableRequest;
		}

		void ProcessKeys(PropertyInfo[] properties, CreateTableRequest createTableRequest)
		{
			foreach (var property in properties) {
				KeySchemaElement keySchemaElement = null;

				var hashKeyAttribute = property.GetCustomAttributes<DynamoDBHashKeyAttribute>().FirstOrDefault(x => x.GetType() == typeof(DynamoDBHashKeyAttribute));
				if (hashKeyAttribute != null) {
					keySchemaElement = BuildKey(property, hashKeyAttribute);
				}

				var rangeKeyAttribute = property.GetCustomAttributes<DynamoDBRangeKeyAttribute>().FirstOrDefault(x => x.GetType() == typeof(DynamoDBRangeKeyAttribute));
				if (rangeKeyAttribute != null) {
					if (keySchemaElement != null) {
						throw new InvalidOperationException("A property can not have both the DynamoDBRangeKeyAttribute and DynamoDBHashKeyAttribute applied");
					}
					keySchemaElement = BuildKey(property, rangeKeyAttribute);
				}

				if (keySchemaElement == null) continue;

				AddAttributeIfMissing(createTableRequest, property, keySchemaElement.AttributeName);
				createTableRequest.KeySchema.Add(keySchemaElement);
			}
		}

		void ProcessGlobalSecondaryIndexHashKeys(Type type, PropertyInfo[] properties, CreateTableRequest createTableRequest)
		{
			foreach (var property in properties) {
				var gsiHashKeyAttribute = property.GetCustomAttribute<DynamoDBGlobalSecondaryIndexHashKeyAttribute>();
				if (gsiHashKeyAttribute == null) continue;
				var key = BuildKey(property, gsiHashKeyAttribute);
				AddAttributeIfMissing(createTableRequest, property, key.AttributeName);
				foreach (var indexName in gsiHashKeyAttribute.IndexNames) {
					var index = SummonGlobalSecondaryIndex(type, properties, createTableRequest, indexName);
					index.KeySchema.Add(key);
				}
			}
		}

		void ProcessGlobalSecondaryIndexRangeKeys(Type type, PropertyInfo[] properties, CreateTableRequest createTableRequest)
		{
			foreach (var property in properties) {
				var gsiRangeKeyAttribute = property.GetCustomAttribute<DynamoDBGlobalSecondaryIndexRangeKeyAttribute>();
				if (gsiRangeKeyAttribute == null) continue;
				var key = BuildKey(property, gsiRangeKeyAttribute);
				AddAttributeIfMissing(createTableRequest, property, key.AttributeName);
				foreach (var indexName in gsiRangeKeyAttribute.IndexNames) {
					var index = SummonGlobalSecondaryIndex(type, properties, createTableRequest, indexName);
					index.KeySchema.Add(key);
				}
			}
		}

		void ProcessLocalSecondaryIndexes(Type type, PropertyInfo[] properties, CreateTableRequest createTableRequest)
		{
			foreach (var property in properties) {
				var lsiRangeKeyAttribute = property.GetCustomAttribute<DynamoDBLocalSecondaryIndexRangeKeyAttribute>();
				if (lsiRangeKeyAttribute == null) continue;
				var key = BuildKey(property, lsiRangeKeyAttribute);
				AddAttributeIfMissing(createTableRequest, property, key.AttributeName);
				foreach (var indexName in lsiRangeKeyAttribute.IndexNames) {
					var index = SummonLocalSecondaryIndex(type, properties, createTableRequest, indexName);
					index.KeySchema.Add(key);
				}
			}
		}

		LocalSecondaryIndex SummonLocalSecondaryIndex(Type type, PropertyInfo[] properties, CreateTableRequest createTableRequest, string indexName)
		{
			var index = createTableRequest.LocalSecondaryIndexes.FirstOrDefault(x => x.IndexName == indexName);

			if (index != null) {
				return index;
			}

			var indexAttribute = GetDynamoIndexAttributeForIndexConstant(type, properties, createTableRequest, indexName);

			index = new LocalSecondaryIndex {
				IndexName = indexName,
				KeySchema = new List<KeySchemaElement> {
					new KeySchemaElement {
						AttributeName = createTableRequest.KeySchema.Single(x=>x.KeyType == KeyType.HASH).AttributeName,
						KeyType = KeyType.HASH
					}
				},
				Projection = new Projection {
					ProjectionType = indexAttribute.AmazonProjectionType,
					NonKeyAttributes = indexAttribute.NonKeyAttributes.EmptyIfNull().ToList()
				}
			};

			createTableRequest.LocalSecondaryIndexes.Add(index);

			return index;
		}


		GlobalSecondaryIndex SummonGlobalSecondaryIndex(Type type, PropertyInfo[] properties, CreateTableRequest createTableRequest, string indexName)
		{
			var index = createTableRequest.GlobalSecondaryIndexes.FirstOrDefault(x => x.IndexName == indexName);

			if (index != null) {
				return index;
			}

			var indexAttribute = GetDynamoIndexAttributeForIndexConstant(type, properties, createTableRequest, indexName);

			index = new GlobalSecondaryIndex {
				IndexName = indexName,
				KeySchema = new List<KeySchemaElement>(),
				Projection = new Projection {
					ProjectionType = indexAttribute.AmazonProjectionType,
					NonKeyAttributes = indexAttribute.NonKeyAttributes.EmptyIfNull().ToList()
				},
				ProvisionedThroughput = new ProvisionedThroughput {
					ReadCapacityUnits = ReadCapacityUnits,
					WriteCapacityUnits = WriteCapacityUnits
				}
			};

			createTableRequest.GlobalSecondaryIndexes.Add(index);

			return index;
		}

		DynamoIndexAttribute GetDynamoIndexAttributeForIndexConstant(Type type, PropertyInfo[] properties, CreateTableRequest createTableRequest, string indexName)
		{
			var constants = type.GetFields(BindingFlags.Public | BindingFlags.Static |
			                               BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly)
				.ToList();

			var indexConstant = constants.FirstOrDefault(x => (x.GetRawConstantValue() as string) == indexName);

			var indexAttribute = indexConstant.GetCustomAttribute<DynamoIndexAttribute>();

			if (indexAttribute == null) {
				throw new Exception($"The index name constant is not decorated with the {nameof(DynamoIndexAttribute)}") {
					Data = {
						["IndexName"] = indexName,
						["TypeName"] = type.Name
					}
				};
			}

			return indexAttribute;
		}

		KeySchemaElement BuildKey(PropertyInfo property, DynamoDBPropertyAttribute attribute)
		{
			if (attribute == null) {
				return null;
			}

			return new KeySchemaElement {
				AttributeName = attribute.AttributeName ?? property.Name,
				KeyType = (attribute is DynamoDBRangeKeyAttribute || attribute is DynamoDBLocalSecondaryIndexRangeKeyAttribute) ? KeyType.RANGE : KeyType.HASH
			};
		}

		void AddAttributeIfMissing(CreateTableRequest request, PropertyInfo property, string attributeName)
		{
			if (request.AttributeDefinitions.Any(x => x.AttributeName == attributeName)) {
				return;
			}

			request.AttributeDefinitions.Add(new AttributeDefinition {
				AttributeName = attributeName,
				AttributeType = ConvertToScalarAttributeType(InferType(property))
			});
		}

		ScalarAttributeType ConvertToScalarAttributeType(DynamoDBEntryType type)
		{
			switch (type) {
				case DynamoDBEntryType.Binary:
					return ScalarAttributeType.S;
				case DynamoDBEntryType.Numeric:
					return ScalarAttributeType.N;
				case DynamoDBEntryType.String:
					return ScalarAttributeType.S;
				default:
					throw new Exception("Unsupported Dynamo entry type") {
						Data = {
							["Type"] = type
						}
					};
			}
		}

		DynamoDBEntryType InferType(PropertyInfo property)
		{
			DynamoDBEntryTypeAttribute typeAttribute = property.GetCustomAttribute<DynamoDBEntryTypeAttribute>();

			if (typeAttribute == null) {
				// we allow custom converters to be decorated with the DynamoDBPropertyAttribute
				// so we can infer what type the converter will be producing
				var propertyAttributes = property.GetCustomAttributes<DynamoDBPropertyAttribute>(true);

				foreach (var propertyAttribute in propertyAttributes) {
					var converterType = propertyAttribute?.Converter;
					if (converterType != null) {
						typeAttribute = converterType.GetTypeInfo().GetCustomAttribute<DynamoDBEntryTypeAttribute>();
						if (typeAttribute != null) {
							break;
						}
					}
				}
			}

			if (typeAttribute != null) {
				return typeAttribute.EntryType;
			}

			if (property.PropertyType == typeof(string)) {
				return DynamoDBEntryType.String;
			}

			if (property.PropertyType == typeof(long) || property.PropertyType == typeof(int)) {
				return DynamoDBEntryType.Numeric;
			}

			if (property.PropertyType == typeof(byte[])) {
				return DynamoDBEntryType.Binary;
			}

			throw new 
				Exception("This code is not smart enough to infer the attribute type from the property type: {property.PropertyType}.  " +
				          "You can resolve this by improving the code or decorating the property with the DynamoDBEntryType attribute.");
		}
	}
}
