using System;

namespace Pushpay.DynamoDbProvisioner
{
	public class DynamoIndexAttribute : Attribute
	{
		public ProjectionType ProjectionType { get; set; }

		public string[] NonKeyAttributes { get; set; }

		public Amazon.DynamoDBv2.ProjectionType AmazonProjectionType {
			get {
				switch (ProjectionType) {
					case ProjectionType.All:
						return Amazon.DynamoDBv2.ProjectionType.ALL;
					case ProjectionType.Include:
						return Amazon.DynamoDBv2.ProjectionType.INCLUDE;
					case ProjectionType.KeysOnly:
						return Amazon.DynamoDBv2.ProjectionType.KEYS_ONLY;
					default:
						throw new Exception($"Unsupported projection type: {ProjectionType}");
				}
			}
		}
	}
}
