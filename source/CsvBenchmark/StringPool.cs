using System;
using System.Text;

namespace CsvBenchmark
{
	internal static class StringPool
	{
		private static readonly string[] Strings = new string[128];

		static StringPool()
		{
			for (int i = 0; i < Strings.Length; i++)
			{
				Strings[i] = ((char)i).ToString();
			}
		}

		internal static string PoolUtf8(ReadOnlySpan<byte> buf)
		{
			if (buf.IsEmpty)
			{
				return string.Empty;
			}

			if (buf.Length == 1 && buf[0] < 128)
			{
				return Strings[buf[0]];
			}

			return Encoding.UTF8.GetString(buf);
		}

		internal static string Pool(char[] buf, int offset, int length)
		{
			if (length == 1)
			{
				var c = buf[offset];
				if (c < 128)
					return Strings[c];
			}
			return new string(buf, offset, length);
		}
	}
}
