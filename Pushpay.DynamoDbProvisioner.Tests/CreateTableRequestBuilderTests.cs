using System;
using System.Linq;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using PowerAssert;
using Pushpay.DynamoDbProvisioner.Tests.Helper;
using Xunit;
using static Amazon.DynamoDBv2.ProjectionType;

namespace Pushpay.DynamoDbProvisioner.Tests
{
	public class CreateTableRequestBuilderTests
	{
		CreateTableRequest _table;

		[Fact]
		public void TableWithStringHashKey()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithStringHashKey))
				.Then(TableNameIs_, "test_TableWithStringHashKey")
				.And(HasHashKey_OfType_, "Id", ScalarAttributeType.S)
				.Execute();
		}

		[Fact]
		public void TableWithStringHashKeyAndNumberRangeKey()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithStringHashKeyAndNumberRangeKey))
				.Then(TableNameIs_, "test_TableWithStringHashKeyAndNumberRangeKey")
				.And(HasHashKey_OfType_, "Id", ScalarAttributeType.S)
				.And(HasRangeKey_OfType_, "Timestamp", ScalarAttributeType.N)
				.Execute();
		}

		[Fact]
		public void TableWithAttributeNamesSpecifiedForKeys()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithAttributeNamesSpecifiedForKeys))
				.Then(TableNameIs_, "test_TableWithAttributeNamesSpecifiedForKeys")
				.And(HasHashKey_OfType_, "Data.Id", ScalarAttributeType.S)
				.And(HasRangeKey_OfType_, "Data.TimeStamp", ScalarAttributeType.N)
				.Execute();
		}

		[Fact]
		public void TableWithGlobalSecondaryWithHashKeyOnly()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithGlobalSecondaryIndexHashKeyOnly))
				.Then(TableNameIs_, "test_TableWithGlobalSecondaryIndexHashKeyOnly")
				.And(HasHashKey_OfType_, "Id", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByEmail-Index", ALL)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByEmail-Index", "Email", ScalarAttributeType.S)
				.Execute();
		}

		[Fact]
		public void TableWithGlobalSecondaryIndexHashKeyAndRangeKey()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithGlobalSecondaryIndexHashKeyAndRangeKey))
				.Then(TableNameIs_, "test_TableWithGlobalSecondaryIndexHashKeyAndRangeKey")
				.And(HasHashKey_OfType_, "Id", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByLastNameAndFirstName-Index", ALL)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByLastNameAndFirstName-Index", "LastName", ScalarAttributeType.S)
				.And(GlobalSecondaryIndex_HasRangeKey_OfType_, "ByLastNameAndFirstName-Index", "FirstName", ScalarAttributeType.S)
				.Execute();
		}

		[Fact]
		public void TableWithTwoGlobalSecondaryIndexes()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithTwoGlobalSecondaryIndexes))
				.Then(TableNameIs_, "test_TableWithTwoGlobalSecondaryIndexes")
				.And(HasHashKey_OfType_, "Id", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByEmail-Index", ALL)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByEmail-Index", "Email", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByLastNameAndFirstName-Index", ALL)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByLastNameAndFirstName-Index", "LastName", ScalarAttributeType.S)
				.And(GlobalSecondaryIndex_HasRangeKey_OfType_, "ByLastNameAndFirstName-Index", "FirstName", ScalarAttributeType.S)
				.Execute();
		}

		[Fact]
		public void TableWithTwoGlobalSecondaryIndexesSharingRangeKeyAttribute()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithTwoGlobalSecondaryIndexesSharingRangeKeyAttribute))
				.Then(TableNameIs_, "test_TableWithTwoGlobalSecondaryIndexesSharingRangeKeyAttribute")
				.And(HasHashKey_OfType_, "Id", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByEmailAndFirstName-Index", ALL)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByEmailAndFirstName-Index", "Email", ScalarAttributeType.S)
				.And(GlobalSecondaryIndex_HasRangeKey_OfType_, "ByEmailAndFirstName-Index", "FirstName", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByLastNameAndFirstName-Index", ALL)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByLastNameAndFirstName-Index", "LastName", ScalarAttributeType.S)
				.And(GlobalSecondaryIndex_HasRangeKey_OfType_, "ByLastNameAndFirstName-Index", "FirstName", ScalarAttributeType.S)
				.Execute();
		}

		[Fact]
		public void TableWithGlobalSecondIndexWithNonKeyAttributesIncluded()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithGlobalSecondIndexWithNonKeyAttributesIncluded))
				.Then(TableNameIs_, "test_TableWithGlobalSecondIndexWithNonKeyAttributesIncluded")
				.And(HasHashKey_OfType_, "Id", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByEmail-Index", INCLUDE)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByEmail-Index", "Email", ScalarAttributeType.S)
				.And(GlobalSecondaryIndex_HasNonKeyAttribute_OfType_, "ByEmail-Index", "Name.First", ScalarAttributeType.S)
				.And(GlobalSecondaryIndex_HasNonKeyAttribute_OfType_, "ByEmail-Index", "Age", ScalarAttributeType.N)
				.Execute();
		}

		[Fact]
		public void TableWithGlobalSecondIndexKeysOnly()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithGlobalSecondIndexKeysOnly))
				.Then(TableNameIs_, "test_TableWithGlobalSecondIndexKeysOnly")
				.And(HasHashKey_OfType_, "Id", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByEmail-Index", KEYS_ONLY)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByEmail-Index", "Email", ScalarAttributeType.S)
				.Execute();
		}

		[Fact]
		public void TableWithLocalSecondaryIndex()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithLocalSecondaryIndex))
				.Then(TableNameIs_, "test_TableWithLocalSecondaryIndex")
				.And(HasHashKey_OfType_, "AccountId", ScalarAttributeType.N)
				.And(HasRangeKey_OfType_, "Recieved", ScalarAttributeType.S)
				.And(LocalSecondaryIndexNamed_ForProjectType_, "ByAccountIdAndFromAddress-Index", ALL)
				.And(LocalSecondaryIndex_HasRangeKey_OfType_, "ByAccountIdAndFromAddress-Index", "From", ScalarAttributeType.S)
				.Execute();
		}

		[Fact]
		public void TableWithLocalSecondaryIndexWithNonKeyAttributesIncluded()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithLocalSecondaryIndexWithNonKeyAttributesIncluded))
				.Then(TableNameIs_, "test_TableWithLocalSecondaryIndexWithNonKeyAttributesIncluded")
				.And(HasHashKey_OfType_, "AccountId", ScalarAttributeType.N)
				.And(HasRangeKey_OfType_, "Recieved", ScalarAttributeType.S)
				.And(LocalSecondaryIndexNamed_ForProjectType_, "ByAccountIdAndFromAddress-Index", INCLUDE)
				.And(LocalSecondaryIndex_HasHashKey_OfType_, "ByAccountIdAndFromAddress-Index", "AccountId", ScalarAttributeType.N)
				.And(LocalSecondaryIndex_HasRangeKey_OfType_, "ByAccountIdAndFromAddress-Index", "From", ScalarAttributeType.S)
				.And(LocalSecondaryIndex_HasNonKeyAttribute_OfType_, "ByAccountIdAndFromAddress-Index", "To", ScalarAttributeType.S)
				.Execute();
		}

		[Fact]
		public void TableWithKeyWhichIsRangeAndHashKeyForDifferentGlobalSecondaryIndexes()
		{
			new SpecFromTestName()
				.When(CreateTableForType_, typeof(TableWithKeyWhichIsRangeAndHashKeyForDifferentGlobalSecondaryIndexes))
				.Then(TableNameIs_, "test_TableWithKeyWhichIsRangeAndHashKeyForDifferentGlobalSecondaryIndexes")
				.And(HasHashKey_OfType_, "Id", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByFirstAndLastName-Index", KEYS_ONLY)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByFirstAndLastName-Index", "FirstName", ScalarAttributeType.S)
				.And(GlobalSecondaryIndex_HasRangeKey_OfType_, "ByFirstAndLastName-Index", "LastName", ScalarAttributeType.S)
				.And(GlobalSecondaryIndexNamed_ForProjectType_, "ByLastNameAndFirstName-Index", KEYS_ONLY)
				.And(GlobalSecondaryIndex_HasHashKey_OfType_, "ByLastNameAndFirstName-Index", "LastName", ScalarAttributeType.S)
				.And(GlobalSecondaryIndex_HasRangeKey_OfType_, "ByLastNameAndFirstName-Index", "FirstName", ScalarAttributeType.S)
				.Execute();
		}

		void CreateTableForType_(Type type)
		{
			_table = new CreateTableRequestBuilder()
				.BuildFrom(type, "test_");
		}

		void TableNameIs_(string name)
		{
			PAssert.IsTrue(() => _table.TableName == name);
		}

		void HasHashKey_OfType_(string name, ScalarAttributeType type)
		{
			var key = _table.KeySchema.Single(x => x.AttributeName == name);
			PAssert.IsTrue(() => key.KeyType == KeyType.HASH);
			var attributeDefinition = _table.AttributeDefinitions.Single(x => x.AttributeName == name);
			PAssert.IsTrue(() => attributeDefinition.AttributeType == type);
		}

		void HasRangeKey_OfType_(string name, ScalarAttributeType type)
		{
			var key = _table.KeySchema.Single(x => x.AttributeName == name);
			PAssert.IsTrue(() => key.KeyType == KeyType.RANGE);
			var attributeDefinition = _table.AttributeDefinitions.Single(x => x.AttributeName == name);
			PAssert.IsTrue(() => attributeDefinition.AttributeType == type);
		}

		void GlobalSecondaryIndexNamed_ForProjectType_(string name, Amazon.DynamoDBv2.ProjectionType type)
		{
			var index = _table.GlobalSecondaryIndexes.Single(x => x.IndexName == name);
			PAssert.IsTrue(() => index.Projection.ProjectionType == type);
		}

		void GlobalSecondaryIndex_HasHashKey_OfType_(string indexName, string keyName, ScalarAttributeType type)
		{
			var index = _table.GlobalSecondaryIndexes.Single(x => x.IndexName == indexName);
			var key = index.KeySchema.Single(x => x.AttributeName == keyName);
			PAssert.IsTrue(() => key.KeyType == KeyType.HASH);
			var attributeDefinition = _table.AttributeDefinitions.Single(x => x.AttributeName == keyName);
			PAssert.IsTrue(() => attributeDefinition.AttributeType == type);
		}

		void GlobalSecondaryIndex_HasRangeKey_OfType_(string indexName, string keyName, ScalarAttributeType type)
		{
			var index = _table.GlobalSecondaryIndexes.Single(x => x.IndexName == indexName);
			var key = index.KeySchema.SingleOrDefault(x => x.AttributeName == keyName);
			PAssert.IsTrue(() => key.KeyType == KeyType.RANGE);
			var attributeDefinition = _table.AttributeDefinitions.Single(x => x.AttributeName == keyName);
			PAssert.IsTrue(() => attributeDefinition.AttributeType == type);
		}

		void GlobalSecondaryIndex_HasNonKeyAttribute_OfType_(string indexName, string attributeName, ScalarAttributeType type)
		{
			var index = _table.GlobalSecondaryIndexes.Single(x => x.IndexName == indexName);
			PAssert.IsTrue(() => index.Projection.NonKeyAttributes.Contains(attributeName));
		}

		void LocalSecondaryIndexNamed_ForProjectType_(string name, Amazon.DynamoDBv2.ProjectionType type)
		{
			var index = _table.LocalSecondaryIndexes.Single(x => x.IndexName == name);
			PAssert.IsTrue(() => index.Projection.ProjectionType == type);
		}

		void LocalSecondaryIndex_HasHashKey_OfType_(string indexName, string keyName, ScalarAttributeType type)
		{
			var index = _table.LocalSecondaryIndexes.Single(x => x.IndexName == indexName);
			var key = index.KeySchema.SingleOrDefault(x => x.AttributeName == keyName);
			PAssert.IsTrue(() => key.KeyType == KeyType.HASH);
			var attributeDefinition = _table.AttributeDefinitions.Single(x => x.AttributeName == keyName);
			PAssert.IsTrue(() => attributeDefinition.AttributeType == type);
		}

		void LocalSecondaryIndex_HasRangeKey_OfType_(string indexName, string keyName, ScalarAttributeType type)
		{
			var index = _table.LocalSecondaryIndexes.Single(x => x.IndexName == indexName);
			var key = index.KeySchema.SingleOrDefault(x => x.AttributeName == keyName);
			PAssert.IsTrue(() => key.KeyType == KeyType.RANGE);
			var attributeDefinition = _table.AttributeDefinitions.Single(x => x.AttributeName == keyName);
			PAssert.IsTrue(() => attributeDefinition.AttributeType == type);
		}

		void LocalSecondaryIndex_HasNonKeyAttribute_OfType_(string indexName, string attributeName, ScalarAttributeType type)
		{
			var index = _table.LocalSecondaryIndexes.Single(x => x.IndexName == indexName);
			PAssert.IsTrue(() => index.Projection.NonKeyAttributes.Contains(attributeName));
		}
	}
}
