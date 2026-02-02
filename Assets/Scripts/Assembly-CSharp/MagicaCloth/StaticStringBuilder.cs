using System.Text;

namespace MagicaCloth
{
	public class StaticStringBuilder
	{
		private static StringBuilder stringBuilder = new StringBuilder(1024);

		public static StringBuilder Instance
		{
			get
			{
				return stringBuilder;
			}
		}

		public static void Clear()
		{
			stringBuilder.Length = 0;
		}

		public static StringBuilder Append(params object[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				stringBuilder.Append(args[i]);
			}
			return stringBuilder;
		}

		public static StringBuilder AppendLine(params object[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				stringBuilder.Append(args[i]);
			}
			stringBuilder.Append("\n");
			return stringBuilder;
		}

		public static StringBuilder AppendLine()
		{
			stringBuilder.Append("\n");
			return stringBuilder;
		}

		public static string AppendToString(params object[] args)
		{
			stringBuilder.Length = 0;
			for (int i = 0; i < args.Length; i++)
			{
				stringBuilder.Append(args[i]);
			}
			return stringBuilder.ToString();
		}

		public new static string ToString()
		{
			return stringBuilder.ToString();
		}
	}
}
