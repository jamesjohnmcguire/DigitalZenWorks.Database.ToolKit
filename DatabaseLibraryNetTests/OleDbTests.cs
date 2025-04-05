/////////////////////////////////////////////////////////////////////////////
// Copyright @ 2006 - 2025 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////

using DigitalZenWorks.Common.Utilities;
using NUnit.Framework;
using System.Collections;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Runtime.Versioning;

namespace DigitalZenWorks.Database.ToolKit.Tests
{
	/// <summary>
	/// Ole db tests class.
	/// </summary>
	[SupportedOSPlatform("windows")]
	[TestFixture]
	internal sealed class OleDbTests
	{
		/// <summary>
		/// Get schema test.
		/// </summary>
		[Test]
		public static void GetSchema()
		{
			string databaseFile = GetTestMdbFile();

			Hashtable tables = DataDefinition.GetSchema(databaseFile);

			int count = tables.Count;

			Assert.That(count, Is.EqualTo(2));

			foreach (System.Collections.DictionaryEntry entry in tables)
			{
				object key = entry.Key;
				object value = entry.Value;
				string name = key.ToString();
				Table table = (Table)value;

				Assert.That(name, Is.AnyOf("AddressesTest2", "AddressesTest"));
				Assert.That(
					table.Name, Is.AnyOf("AddressesTest2", "AddressesTest"));
			}
		}

		/// <summary>
		/// GetTableColumns test.
		/// </summary>
		[Test]
		public void GetTableColumns()
		{
			string tableName = "AddressTest";
			string databaseFile = GetTestMdbFile();

			using OleDbSchema oleDbSchema = new(databaseFile);

			DataTable table = oleDbSchema.GetTableColumns(tableName);

			Assert.That(table, Is.Not.Null);

			string name = table.TableName;
			Assert.That(name, Is.EqualTo("Columns"));
		}

		private static string GetTestMdbFile()
		{
			string resource =
				"DigitalZenWorks.Database.ToolKit.Tests.test.mdb";

			string fileName = Path.GetTempFileName();

			// A 0 byte sized file is created.  Need to remove it.
			File.Delete(fileName);
			string filePath = Path.ChangeExtension(fileName, "mdb");

			bool result = FileUtils.CreateFileFromEmbeddedResource(
				resource, filePath);

			Assert.That(result, Is.True);

			result = File.Exists(filePath);
			Assert.That(result, Is.True);

			return filePath;
		}
	}
}
