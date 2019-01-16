using Amazon.DynamoDBv2.DataModel;

namespace Pushpay.DynamoDbProvisioner.Tests
{
	[DynamoDBTable(nameof(TableWithStringHashKey))]
	public class TableWithStringHashKey
	{
		[DynamoDBHashKey]
		public string Id { get; set; }

		public string FullName { get; set; }
	}

	[DynamoDBTable(nameof(TableWithStringHashKeyAndNumberRangeKey))]
	public class TableWithStringHashKeyAndNumberRangeKey
	{
		[DynamoDBHashKey]
		public string Id { get; set; }

		[DynamoDBRangeKey]
		public int Timestamp { get; set; }
	}

	[DynamoDBTable(nameof(TableWithAttributeNamesSpecifiedForKeys))]
	public class TableWithAttributeNamesSpecifiedForKeys
	{
		[DynamoDBHashKey(AttributeName = "Data.Id")]
		public string Id { get; set; }

		[DynamoDBRangeKey("Data.TimeStamp")]
		public int Timestamp { get; set; }
	}

	[DynamoDBTable(nameof(TableWithGlobalSecondaryIndexHashKeyOnly))]
	public class TableWithGlobalSecondaryIndexHashKeyOnly
	{
		[DynamoIndex(ProjectionType = ProjectionType.All)] public const string ByEmailIndex = "ByEmail-Index";

		[DynamoDBHashKey]
		public string Id { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByEmailIndex)]
		public string Email { get; set; }
	}

	[DynamoDBTable(nameof(TableWithGlobalSecondaryIndexHashKeyAndRangeKey))]
	public class TableWithGlobalSecondaryIndexHashKeyAndRangeKey
	{
		[DynamoIndex(ProjectionType = ProjectionType.All)] public const string ByLastNameAndFirstNameIndex = "ByLastNameAndFirstName-Index";

		[DynamoDBHashKey]
		public string Id { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByLastNameAndFirstNameIndex)]
		public string LastName { get; set; }

		[DynamoDBGlobalSecondaryIndexRangeKey(ByLastNameAndFirstNameIndex)]
		public string FirstName { get; set; }
	}

	[DynamoDBTable(nameof(TableWithTwoGlobalSecondaryIndexes))]
	public class TableWithTwoGlobalSecondaryIndexes
	{
		[DynamoIndex(ProjectionType = ProjectionType.All)] public const string ByLastNameAndFirstNameIndex = "ByLastNameAndFirstName-Index";

		[DynamoIndex(ProjectionType = ProjectionType.All)] public const string ByEmailIndex = "ByEmail-Index";

		[DynamoDBHashKey]
		public string Id { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByLastNameAndFirstNameIndex)]
		public string LastName { get; set; }

		[DynamoDBGlobalSecondaryIndexRangeKey(ByLastNameAndFirstNameIndex)]
		public string FirstName { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByEmailIndex)]
		public string Email { get; set; }
	}

	[DynamoDBTable(nameof(TableWithTwoGlobalSecondaryIndexesSharingRangeKeyAttribute))]
	public class TableWithTwoGlobalSecondaryIndexesSharingRangeKeyAttribute
	{
		[DynamoIndex(ProjectionType = ProjectionType.All)] public const string ByEmailAndFirstNameIndex = "ByEmailAndFirstName-Index";

		[DynamoIndex(ProjectionType = ProjectionType.All)] public const string ByLastNameAndFirstNameIndex = "ByLastNameAndFirstName-Index";

		[DynamoDBHashKey]
		public string Id { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByLastNameAndFirstNameIndex)]
		public string LastName { get; set; }

		[DynamoDBGlobalSecondaryIndexRangeKey(ByLastNameAndFirstNameIndex, ByEmailAndFirstNameIndex)]
		public string FirstName { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByEmailAndFirstNameIndex)]
		public string Email { get; set; }
	}

	[DynamoDBTable(nameof(TableWithGlobalSecondIndexWithNonKeyAttributesIncluded))]
	public class TableWithGlobalSecondIndexWithNonKeyAttributesIncluded
	{
		[DynamoIndex(ProjectionType = ProjectionType.Include,
			NonKeyAttributes = new[] {"Name.First", nameof(Age)})] public const string ByEmailIndex = "ByEmail-Index";

		[DynamoDBHashKey]
		public string Id { get; set; }

		public int Age { get; set; }

		[DynamoDBProperty("Name.First")]
		public string FirstName { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByEmailIndex)]
		public string Email { get; set; }
	}

	[DynamoDBTable(nameof(TableWithGlobalSecondIndexKeysOnly))]
	public class TableWithGlobalSecondIndexKeysOnly
	{
		[DynamoIndex(ProjectionType = ProjectionType.KeysOnly)] public const string ByEmailIndex = "ByEmail-Index";

		[DynamoDBHashKey]
		public string Id { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByEmailIndex)]
		public string Email { get; set; }
	}

	[DynamoDBTable(nameof(TableWithLocalSecondaryIndex))]
	public class TableWithLocalSecondaryIndex
	{
		[DynamoIndex(ProjectionType = ProjectionType.All)] public const string ByAccountIdAndFromAddressIndex = "ByAccountIdAndFromAddress-Index";

		[DynamoDBHashKey]
		public long AccountId { get; set; }

		[DynamoDBRangeKey]
		public string Recieved { get; set; }

		[DynamoDBLocalSecondaryIndexRangeKey(ByAccountIdAndFromAddressIndex)]
		public string From { get; set; }
	}

	[DynamoDBTable(nameof(TableWithLocalSecondaryIndexWithNonKeyAttributesIncluded))]
	public class TableWithLocalSecondaryIndexWithNonKeyAttributesIncluded
	{
		[DynamoIndex(ProjectionType = ProjectionType.Include,
			NonKeyAttributes = new[] {nameof(To)})] public const string ByAccountIdAndFromAddressIndex = "ByAccountIdAndFromAddress-Index";

		[DynamoDBHashKey]
		public long AccountId { get; set; }

		[DynamoDBRangeKey]
		public string Recieved { get; set; }

		[DynamoDBLocalSecondaryIndexRangeKey(ByAccountIdAndFromAddressIndex)]
		public string From { get; set; }

		public string To { get; set; }

		public string Subject { get; set; }

		public string Body { get; set; }
	}

	[DynamoDBTable(nameof(TableWithKeyWhichIsRangeAndHashKeyForDifferentGlobalSecondaryIndexes))]
	public class TableWithKeyWhichIsRangeAndHashKeyForDifferentGlobalSecondaryIndexes
	{
		[DynamoIndex(ProjectionType = ProjectionType.KeysOnly)] public const string ByFirstNameAndLastNameIndex = "ByFirstAndLastName-Index";

		[DynamoIndex(ProjectionType = ProjectionType.KeysOnly)] public const string ByLastNameAndFirstNameIndex = "ByLastNameAndFirstName-Index";

		[DynamoDBHashKey]
		public string Id { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByLastNameAndFirstNameIndex)]
		[DynamoDBGlobalSecondaryIndexRangeKey(ByFirstNameAndLastNameIndex)]
		public string LastName { get; set; }

		[DynamoDBGlobalSecondaryIndexHashKey(ByFirstNameAndLastNameIndex)]
		[DynamoDBGlobalSecondaryIndexRangeKey(ByLastNameAndFirstNameIndex)]
		public string FirstName { get; set; }
	}

	[DynamoDBTable(nameof(TableWithTtlProperty))]
	public class TableWithTtlProperty
	{
		[DynamoDBHashKey]
		public string SessionId { get; set; }

		public string UserId { get; set; }

		[DynamoDBTimeToLive]
		public long Expiry { get; set; }
	}

	[DynamoDBTable(nameof(SimplePerson))]
	public class SimplePerson
	{
		[DynamoIndex(ProjectionType = ProjectionType.All)]
		public const string FirstNameLastNameIndex = "FirstNameLastName-Index";

		[DynamoIndex(ProjectionType = ProjectionType.KeysOnly)]
		public const string LastNameAgeIndex = "LastNameAge-Index";

		[DynamoDBHashKey]
		[DynamoDBGlobalSecondaryIndexRangeKey(FirstNameLastNameIndex)]
		public string LastName { get; set; }

		[DynamoDBRangeKey]
		[DynamoDBGlobalSecondaryIndexHashKey(FirstNameLastNameIndex)]
		public string FirstName { get; set; }

		[DynamoDBLocalSecondaryIndexRangeKey(LastNameAgeIndex)]
		public int Age { get; set; }

		public int CatsOwned { get; set; }
	}
}
