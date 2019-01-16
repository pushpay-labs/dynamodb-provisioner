using System;
using System.Linq;
using System.Reflection;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

namespace Pushpay.DynamoDbProvisioner
{
	internal class DynamoTimeToLiveRequestBuilder
	{
		public UpdateTimeToLiveRequest BuildFrom(Type tableType, string tablePrefix)
		{
			var propAndAttribute = tableType.GetProperties()
				.Select(prop => new {
					property = prop,
					ttlAttribute = prop.GetCustomAttribute<DynamoDBTimeToLiveAttribute>()
				})
				.SingleOrDefault(x => x.ttlAttribute != null);

			if (propAndAttribute == null) {
				return null;
			}

			return new UpdateTimeToLiveRequest {
				TableName = tablePrefix + tableType.GetTypeInfo().GetCustomAttribute<DynamoDBTableAttribute>()?.TableName,
				TimeToLiveSpecification = new TimeToLiveSpecification {
					AttributeName = propAndAttribute.property.GetCustomAttribute<DynamoDBRenamableAttribute>()?.AttributeName ?? propAndAttribute.property.Name,
					Enabled = true
				}
			};
		}
	}
}
