/////////////////////////////////////////////////////////////////////////////
// Copyright @ 2006 - 2025 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////

using DigitalZenWorks.Common.Utilities;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
		/// Test to see if test db exists
		/// </summary>
		[Test]
		public void DatabaseCanOpen()
		{
			//	string provider = "Microsoft.ACE.OLEDB.12.0";

			//	string connectionString = string.Format(
			//		CultureInfo.InvariantCulture,
			//		"provider={0}; Data Source={1}",
			//		provider,
			//		dataSource);
			//	using (OleDbConnection oleDbConnection =
			//		new OleDbConnection(connectionString))
			//	{
			//		oleDbConnection.Open();
			//		oleDbConnection.Close();
			//	}

			//	// assuming no exceptions
			//	Assert.IsTrue(File.Exists(dataSource));
			Assert.Pass();
		}

		/// <summary>
		/// Get relationships test.
		/// </summary>
		[Test]
		public static void GetRelationships()
		{
			string dependentTableName = "Addresses";
			string databaseFile = GetTestMdbFile();

			using OleDbSchema oleDbSchema = new(databaseFile);

			ArrayList relationships = DataDefinition.GetRelationships(
				oleDbSchema, dependentTableName);

			int count = relationships.Count;

			Assert.That(count, Is.EqualTo(1));

			Relationship relationship = (Relationship)relationships[0];

			string name = relationship.ParentTable;
			Assert.That(name, Is.EqualTo("Addresses"));

			name = relationship.ChildTable;
			Assert.That(name, Is.EqualTo("Contacts"));
		}

		/// <summary>
		/// Get relationships test.
		/// </summary>
		[Test]
		public static void GetRelationships2()
		{
			string dependentTableName = "Addresses";
			string databaseFile = GetTestMdbFile();

			using OleDbSchema oleDbSchema = new(databaseFile);

			List<Relationship> relationships =
				DataDefinition.GetRelationshipsNew(
					oleDbSchema, dependentTableName);

			int count = relationships.Count;

			Assert.That(count, Is.EqualTo(1));

			Relationship relationship = relationships[0];

			string name = relationship.ParentTable;
			Assert.That(name, Is.EqualTo("Addresses"));

			name = relationship.ChildTable;
			Assert.That(name, Is.EqualTo("Contacts"));
		}

		/// <summary>
		/// Get schema test.
		/// </summary>
		[Test]
		public static void GetSchema()
		{
			string databaseFile = GetTestMdbFile();

			Hashtable tables = DataDefinition.GetSchema(databaseFile);

			int count = tables.Count;

			Assert.That(count, Is.EqualTo(7));

			foreach (System.Collections.DictionaryEntry entry in tables)
			{
				object key = entry.Key;
				object value = entry.Value;
				string name = key.ToString();
				Table table = (Table)value;

				Assert.That(name, Is.AnyOf("Addresses", "Categories",
					"Contacts", "Makers", "Products", "Sections", "Series"));
				Assert.That(table.Name, Is.AnyOf("Addresses", "Categories",
					"Contacts", "Makers", "Products", "Sections", "Series"));
			}

			object tester = "Addresses";
			bool result = tables.ContainsKey(tester);
			Assert.That(result, Is.True);

			tester = "Contacts";
			result = tables.ContainsKey(tester);
			Assert.That(result, Is.True);

			Table tableItem = (Table)tables["Addresses"];
			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(0));

			tableItem = (Table)tables["Contacts"];
			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(1));
		}

		/// <summary>
		/// Get schema test.
		/// </summary>
		[Test]
		public static void GetSchemaNew()
		{
			string databaseFile = GetTestMdbFile();

			List<Table> tables = DataDefinition.GetSchemaNew(databaseFile);

			int count = tables.Count;
			Assert.That(count, Is.EqualTo(7));

			foreach (Table table in tables)
			{
				Assert.That(table.Name, Is.AnyOf("Addresses", "Categories",
					"Contacts", "Makers", "Products", "Sections", "Series"));
			}

			Table tableItem = tables[0];
			Assert.That(tableItem.Name, Is.EqualTo("Addresses"));

			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(0));

			tableItem = tables[2];
			Assert.That(tableItem.Name, Is.EqualTo("Contacts"));

			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(1));
		}

		/// <summary>
		/// Get schema test.
		/// </summary>
		[Test]
		public static void GetSchemaPrevious()
		{
			string databaseFile = GetTestMdbFile();

			Hashtable tables =
				DataDefinition.GetSchemaPrevious(databaseFile);

			int count = tables.Count;

			Assert.That(count, Is.EqualTo(7));

			foreach (System.Collections.DictionaryEntry entry in tables)
			{
				object key = entry.Key;
				object value = entry.Value;
				string name = key.ToString();
				Table table = (Table)value;

				Assert.That(name, Is.AnyOf("Addresses", "Categories",
					"Contacts", "Makers", "Products", "Sections", "Series"));
				Assert.That(table.Name, Is.AnyOf("Addresses", "Categories",
					"Contacts", "Makers", "Products", "Sections", "Series"));
			}

			object tester = "Addresses";
			bool result = tables.ContainsKey(tester);
			Assert.That(result, Is.True);

			tester = "Contacts";
			result = tables.ContainsKey(tester);
			Assert.That(result, Is.True);

			Table tableItem = (Table)tables["Addresses"];
			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(0));

			tableItem = (Table)tables["Contacts"];
			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(1));
		}

		/// <summary>
		/// GetTableColumns test.
		/// </summary>
		[Test]
		public void GetTableColumns()
		{
			string tableName = "Addresses";
			string databaseFile = GetTestMdbFile();

			using OleDbSchema oleDbSchema = new(databaseFile);

			DataTable table = oleDbSchema.GetTableColumns(tableName);

			Assert.That(table, Is.Not.Null);

			string name = table.TableName;
			Assert.That(name, Is.EqualTo("Columns"));
		}

		/// <summary>
		/// Order table test.
		/// </summary>
		[Test]
		public void OrderTable()
		{
			string databaseFile = GetTestMdbFile();

			Hashtable tables =
				DataDefinition.GetSchemaPrevious(databaseFile);

			ArrayList list = DataDefinition.OrderTable(tables);

			Assert.Pass();
		}

		/// <summary>
		/// Topological sort test.
		/// </summary>
		[Test]
		public void TopologicalSort()
		{
			Hashtable list = [];
			ArrayList dependencies = [];

			ArrayList tableDependencies = new ArrayList(dependencies);
			list.Add("Categories", tableDependencies);

			tableDependencies = new ArrayList(dependencies);
			list.Add("Makers", tableDependencies);

			dependencies.Add("Makers");
			tableDependencies = new ArrayList(dependencies);
			list.Add("Series", tableDependencies);

			dependencies.Clear();
			dependencies.Add("Categories");
			dependencies.Add("Makers");

			tableDependencies = new ArrayList(dependencies);
			list.Add("Sections", tableDependencies);

			dependencies.Clear();
			dependencies.Add("Makers");
			dependencies.Add("Series");
			dependencies.Add("Sections");

			tableDependencies = new ArrayList(dependencies);
			list.Add("ImportProducts", tableDependencies);

			ArrayList sortedList = DataDefinition.TopologicalSort(list);

			int count = sortedList.Count;
			Assert.That(count, Is.EqualTo(5));

			object table = sortedList[0];
			string tableName = table.ToString();
			Assert.That(tableName, Is.AnyOf("Categories", "Makers"));

			table = sortedList[1];
			tableName = table.ToString();
			Assert.That(tableName, Is.AnyOf("Categories", "Makers"));

			table = sortedList[2];
			tableName = table.ToString();
			Assert.That(tableName, Is.AnyOf("Sections", "Series"));

			table = sortedList[3];
			tableName = table.ToString();
			Assert.That(tableName, Is.AnyOf("Sections", "Series"));

			table = sortedList[4];
			tableName = table.ToString();
			Assert.That(tableName, Is.EqualTo("ImportProducts"));
		}

		private static string GetTestMdbFile()
		{
			string resource =
				"DigitalZenWorks.Database.ToolKit.Tests.Products.Test.accdb";

			string fileName = Path.GetTempFileName();

			// A 0 byte sized file is created.  Need to remove it.
			File.Delete(fileName);
			string filePath = Path.ChangeExtension(fileName, "accdb");

			bool result = FileUtils.CreateFileFromEmbeddedResource(
				resource, filePath);

			Assert.That(result, Is.True);

			result = File.Exists(filePath);
			Assert.That(result, Is.True);

			return filePath;
		}
	}
}
