﻿namespace CsvBenchmark
{
	internal static class StringPool
	{
		internal static readonly string[] Strings = new string[128];

		static StringPool()
		{
			for (int i = 0; i < Strings.Length; i++)
			{
				Strings[i] = ((char)i).ToString();
			}
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
