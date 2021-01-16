using BenchmarkDotNet.Attributes;
using Cursively;
using FluentCsv.FluentReader;
using Microsoft.VisualBasic.FileIO;
using Sylvan.Data.Csv;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using static fastCSV;

namespace CsvBenchmark
{
	[MemoryDiagnoser]
	public class CsvReaderBenchmarks
	{
		// buffer size for libraries that allow configuration
		const int BufferSize = 0x10000;

		[Benchmark(Baseline = true)]
		public void NaiveBroken()
		{
			var tr = TestData.GetTextReader();
			string line;
			while ((line = tr.ReadLine()) != null)
			{
				var cols = line.Split(',');
				for (int i = 0; i < cols.Length; i++)
				{
					var s = cols[i];
				}
			}
		}

		[Benchmark]
		public void CsvHelperCsv()
		{
			var tr = TestData.GetTextReader();
			var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.CurrentCulture)
			{
				BufferSize = BufferSize
			};

			var r = new CsvHelper.CsvReader(tr, config);
			var dr = new CsvHelper.CsvDataReader(r);
			dr.ProcessStrings();
		}

		[Benchmark]
		public void NotVBTextFieldParser()
		{
			var tr = TestData.GetTextReader();
			var csv = new NotVisualBasic.FileIO.CsvTextFieldParser(tr);

			while (!csv.EndOfData)
			{
				var fields = csv.ReadFields();
			}
		}

		[Benchmark]
		public void FastCsvParser()
		{
			var s = TestData.GetUtf8Stream();
			var config = new CsvParser.CsvReader.Config() { ReadinBufferSize = BufferSize };
			var csv = new CsvParser.CsvReader(s, System.Text.Encoding.UTF8, config);
			while (csv.MoveNext())
			{
				var row = csv.Current;
				for (int i = 0; i < row.Count; i++)
				{
					var str = row[i];
				}
			}
		}

		[Benchmark]
		public void CsvBySteve()
		{
			var s = TestData.GetUtf8Stream();
			var rows = global::Csv.CsvReader.ReadFromStream(s);

			foreach (var row in rows)
			{
				for (int i = 0; i < row.ColumnCount; i++)
				{
					var str = row[i];
				}
			}
		}

		[Benchmark]
		public void Lumenworks()
		{
			var tr = TestData.GetTextReader();
			var dr = new LumenWorks.Framework.IO.Csv.CsvReader(tr, true, BufferSize);
			dr.ProcessStrings();
		}

		[Benchmark]
		public void NLightCsv()
		{
			var tr = TestData.GetTextReader();
			var dr = new NLight.IO.Text.DelimitedRecordReader(tr, BufferSize);
			dr.ProcessStrings();
		}

		[Benchmark]
		public void VisualBasic()
		{
			var tr = TestData.GetTextReader();
			var dr = new TextFieldParser(tr);
			dr.SetDelimiters(",");
			dr.HasFieldsEnclosedInQuotes = true;

			while (!dr.EndOfData)
			{
				var cols = dr.ReadFields();
				for (int i = 0; i < cols.Length; i++)
				{
					var s = cols[i];
				}
			}
		}

		[Benchmark] // most people won't be able to run this.
		public void OleDbCsv()
		{
			// NOTE: I don't know how to disable the column type detection here
			// so this is a bit inconsistent with other libraries in that it doesn't 
			// process raw string values.

			//Requires: https://www.microsoft.com/en-us/download/details.aspx?id=54920
			var connString = string.Format(
				@"Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
				Path.GetDirectoryName(Path.GetFullPath(TestData.DataFile))
			);
			using var conn = new OleDbConnection(connString);
			conn.Open();
			var cmd = conn.CreateCommand();
			cmd.CommandText = "SELECT * FROM [" + Path.GetFileName(TestData.DataFile) + "]";
			var dr = cmd.ExecuteReader();

			dr.ProcessValues();
		}

		[Benchmark]
		public void FlatFilesCsv()
		{
			var tr = TestData.GetTextReader();
			var opts = new FlatFiles.SeparatedValueOptions() { IsFirstRecordSchema = true };
			var dr = new FlatFiles.FlatFileDataReader(new FlatFiles.SeparatedValueReader(tr, opts));
			dr.ProcessValues();
		}

		[Benchmark]
		public void FSharpData()
		{
			var tr = TestData.GetTextReader();
			var csv = FSharp.Data.CsvFile.Load(tr);

			foreach (var row in csv.Rows)
			{
				for (int i = 0; i < row.Columns.Length; i++)
				{
					var s = row.Columns[i];
				}
			}
		}

		[Benchmark]
		public void Fluent()
		{
			var tr = TestData.GetTextReader();

			var splitter = new global::FluentCsv.CsvParser.Splitters.Rfc4180DataSplitter();
			string line;
			while ((line = tr.ReadLine()) != null)
			{
				var row = splitter.SplitColumns(line, ",");
			}
		}

		[Benchmark]
		public void MgholamFastCSV()
		{
			string[] values = null;
			var rows =
				fastCSV.ReadStream<object>(
					TestData.GetTextReader(),
					',',
					(object obj, COLUMNS cols) =>
					{
						var c = cols.Count;
						if (values == null)
							values = new string[c];
						for (int i = 0; i < c; i++)
						{
							values[i] = cols[i];
						}
						return false;
					}
				);
		}

		[Benchmark]
		public void CursivelyCsvNoPooling()
		{
			var d = TestData.GetUtf8Array();
			var proc = new CursivelyStringVisitor(false);
			CsvSyncInput
				.ForMemory(d)
				.Process(proc);
		}

		[Benchmark]
		public void CursivelyCsvWithPooling()
		{
			var d = TestData.GetUtf8Array();
			var proc = new CursivelyStringVisitor(true);
			CsvSyncInput
				.ForMemory(d)
				.Process(proc);
		}

		[Benchmark]
		public void CtlData()
		{
			var s = TestData.GetTextReader();
			var opts = new Ctl.Data.CsvObjectOptions()
			{
				BufferLength = BufferSize,
				Separator = ',',
				ReadHeader = true,
			};
			var csv = new Ctl.Data.CsvReader(s, opts);

			while (csv.Read())
			{
				var row = csv.CurrentRow;
				var c = row.Count;
				for (int i = 0; i < c; i++)
				{
					var str = row[i].Value;
				}
			}
		}

		[Benchmark]
		public void NReco()
		{
			var tr = TestData.GetTextReader();
			var dr = new NReco.Csv.CsvReader(tr);
			dr.BufferSize = BufferSize;
			dr.Read(); // strip the header row
			while (dr.Read())
			{
				for (int i = 0; i < dr.FieldsCount; i++)
				{
					var s = dr[i];
				}
			}
		}

		[Benchmark]
		public void SylvanNoPooling()
		{
			using var tr = TestData.GetTextReader();
			var opts = new CsvDataReaderOptions { BufferSize = BufferSize };
			using var dr = CsvDataReader.Create(tr, opts);
			dr.ProcessStrings();
		}

		[Benchmark]
		public void SylvanWithPooling()
		{
			using var tr = TestData.GetTextReader();
			var opts = new CsvDataReaderOptions { BufferSize = BufferSize, StringFactory = StringPool.Pool };
			using var dr = CsvDataReader.Create(tr, opts);
			dr.ProcessStrings();
		}
	}
}
