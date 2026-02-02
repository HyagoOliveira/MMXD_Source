#define RELEASE
using System.Diagnostics;

namespace MagicaCloth
{
	public static class Develop
	{
		[Conditional("MAGICACLOTH_DEBUG")]
		public static void Log(string str)
		{
			Debug.Log(str);
		}
	}
}
