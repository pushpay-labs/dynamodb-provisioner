using System;
using Amazon.DynamoDBv2.DocumentModel;

namespace Pushpay.DynamoDbProvisioner
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
	public class DynamoDBEntryTypeAttribute : Attribute
	{
		public DynamoDBEntryType EntryType { get; set; }
	}
}
