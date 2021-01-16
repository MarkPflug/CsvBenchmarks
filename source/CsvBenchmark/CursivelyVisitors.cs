using Cursively;
using System;
using System.Buffers.Text;
using System.Text;

namespace CsvBenchmark
{

	class CursivelyStringVisitor : CsvReaderVisitorBase
	{
		readonly bool doPooling;
		readonly byte[] bytes = new byte[1024];
		int bytesUsed = 0;

		// in any realistic scenario we'd need to at least know the column oridnal to do anything with the record
		int ordinal = 0;

		public CursivelyStringVisitor(bool doPooling)
		{
			this.doPooling = doPooling;
		}

		public override void VisitEndOfField(System.ReadOnlySpan<byte> chunk)
		{
			if (bytesUsed != 0)
			{
				chunk.CopyTo(bytes.AsSpan(bytesUsed, chunk.Length));
				chunk = new ReadOnlySpan<byte>(bytes, 0, bytesUsed + chunk.Length);
				bytesUsed = 0;
			}
			var str = doPooling
				? StringPool.PoolUtf8(chunk)
				: Encoding.UTF8.GetString(chunk);
			ordinal++;
		}

		public override void VisitEndOfRecord()
		{
			ordinal = 0;
		}

		public override void VisitPartialFieldContents(System.ReadOnlySpan<byte> chunk)
		{
			chunk.CopyTo(bytes.AsSpan(bytesUsed, chunk.Length));
			bytesUsed += chunk.Length;
		}
	}

	class CursivelySelectVisitor : CsvReaderVisitorBase
	{
		readonly byte[] bytes = new byte[1024];
		int bytesUsed = 0;

		int ordinal = 0;
		int row = 0;

		int id;
		string name;
		int value;

		public override void VisitEndOfField(ReadOnlySpan<byte> chunk)
		{
			if (bytesUsed != 0)
			{
				chunk.CopyTo(bytes.AsSpan(bytesUsed, chunk.Length));
				chunk = new ReadOnlySpan<byte>(bytes, 0, bytesUsed + chunk.Length);
				bytesUsed = 0;
			}
			if (row != 0) // skip the header row
			{
				switch (ordinal)
				{
					case 0:
						if (!Utf8Parser.TryParse(chunk, out id, out _))
						{
							throw new FormatException();
						}
						break;
					case 10:
						name = Encoding.UTF8.GetString(chunk);
						break;
					case 20:
						if (!Utf8Parser.TryParse(chunk, out value, out _))
						{
							throw new FormatException();
						}
						break;
				}
			}
			ordinal++;
		}

		public override void VisitEndOfRecord()
		{
			if (row > 0)
			{
				// presumably, at this point something would be done with the field values collected for this row.
				// Dealing with the data as an IEnumerable, for example would be difficult
			}
			ordinal = 0;
			row++;
		}

		public override void VisitPartialFieldContents(System.ReadOnlySpan<byte> chunk)
		{
			if (row > 0)
			{
				chunk.CopyTo(bytes.AsSpan(bytesUsed, chunk.Length));
				bytesUsed += chunk.Length;
			}
		}
	}
}
