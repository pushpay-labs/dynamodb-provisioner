using System;

namespace Pushpay.DynamoDbProvisioner
{
	[AttributeUsage(AttributeTargets.Property)]
	public class DynamoDBTimeToLiveAttribute : Attribute { }
}
