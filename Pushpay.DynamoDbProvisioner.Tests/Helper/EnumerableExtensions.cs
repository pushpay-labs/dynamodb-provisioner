using System.Collections.Generic;
using System.Linq;

namespace Pushpay.DynamoDbProvisioner.Tests.Helper
{
	internal static class EnumerableExtensions
	{
		public static bool In<T>(this T item, params T[] items)
		{
			return items.Contains(item);
		}
	}
}
