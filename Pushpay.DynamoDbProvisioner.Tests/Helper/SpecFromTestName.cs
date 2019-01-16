using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using SpecLight;

namespace Pushpay.DynamoDbProvisioner.Tests.Helper
{
	/// <summary>
	///     Creates a Speclight Spec with the description inferred from the calling test method name. Saves time when you're
	///     not going the full BDD route.
	/// </summary>
	public class SpecFromTestName : Spec
	{
		public SpecFromTestName(string moreContext = null, [CallerMemberName] string letTheCompilerProvideThisTestName = null) :
			base(BuildFullDescription(letTheCompilerProvideThisTestName, moreContext))
		{
		}

		static string BuildFullDescription(string testName, string moreContext)
		{
			var nd = BuildDescriptionFromName(testName);
			if (moreContext != null)
			{
				nd += ":\n" + moreContext;
			}
			return nd;
		}

		static string BuildDescriptionFromName(string testName)
		{
			var snakeCased = Regex.Replace(testName ?? "null??", @"\B[A-Z]", x => "_" + x.Value.ToLowerInvariant());

			var stringComponents = snakeCased.Replace('_', ' ')
			                                 .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries); // eliminate long snakes

			return string.Join(" ", stringComponents);
		}
	}
}
