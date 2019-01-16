using System.Collections.Generic;
using System.Linq;

namespace Pushpay.DynamoDbProvisioner.Infrastructure
{
	internal static class EnumerableExtensions
	{
		/// <summary>
		///     If source is null, returns an empty enumerable
		/// </summary>
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> source)
		{
			return source ?? Enumerable.Empty<T>();
		}
	}
}
