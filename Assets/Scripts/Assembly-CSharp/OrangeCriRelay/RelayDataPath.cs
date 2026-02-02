using System.Text;

namespace OrangeCriRelay
{
	public class RelayDataPath
	{
		private static string DataName = "RelayParam";

		private static string platform = "Android";

		public static string GetLoadPath()
		{
			return new StringBuilder(string.Empty).Append(ManagedSingleton<ServerConfig>.Instance.PatchUrl).Append("CriWare/").Append(platform)
				.Append("/Assets/StreamingAssets/")
				.Append("RelayParam")
				.ToString();
		}
	}
}
