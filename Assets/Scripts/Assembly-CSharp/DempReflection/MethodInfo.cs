using System.Diagnostics;
using System.Reflection;

namespace DempReflection
{
	internal class MethodInfo
	{
		public static string GetCurrentMethodInfo()
		{
			return string.Concat(string.Concat("" + "Namespace:" + MethodBase.GetCurrentMethod().DeclaringType.Namespace + "\n", "class Name:", MethodBase.GetCurrentMethod().DeclaringType.FullName, "\n"), "Method:", MethodBase.GetCurrentMethod().Name, "\n");
		}

		public static string GetParentInfo()
		{
			MethodBase method = new StackTrace(true).GetFrame(1).GetMethod();
			return string.Concat(string.Concat(string.Concat("" + method.DeclaringType.Namespace + "\n", method.DeclaringType.Name, "\n"), method.DeclaringType.FullName, "\n"), method.Name, "\n");
		}
	}
}
