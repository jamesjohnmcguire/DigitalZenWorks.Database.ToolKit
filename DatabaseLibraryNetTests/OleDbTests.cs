﻿/////////////////////////////////////////////////////////////////////////////
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
		/// Get relationships test.
		/// </summary>
		[Test]
		public static void GetRelationships()
		{
			string dependentTableName = "AddressesTest";
			string databaseFile = GetTestMdbFile();

			using OleDbSchema oleDbSchema = new(databaseFile);

			ArrayList relationships = DataDefinition.GetRelationships(
				oleDbSchema, dependentTableName);

			int count = relationships.Count;

			Assert.That(count, Is.EqualTo(1));

			Relationship relationship = (Relationship)relationships[0];

			string name = relationship.ParentTable;
			Assert.That(name, Is.EqualTo("AddressesTest"));

			name = relationship.ChildTable;
			Assert.That(name, Is.EqualTo("AddressesTest2"));
		}

		/// <summary>
		/// Get relationships test.
		/// </summary>
		[Test]
		public static void GetRelationships2()
		{
			string dependentTableName = "AddressesTest";
			string databaseFile = GetTestMdbFile();

			using OleDbSchema oleDbSchema = new(databaseFile);

			List<Relationship> relationships =
				DataDefinition.GetRelationshipsNew(
					oleDbSchema, dependentTableName);

			int count = relationships.Count;

			Assert.That(count, Is.EqualTo(1));

			Relationship relationship = relationships[0];

			string name = relationship.ParentTable;
			Assert.That(name, Is.EqualTo("AddressesTest"));

			name = relationship.ChildTable;
			Assert.That(name, Is.EqualTo("AddressesTest2"));
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

			object tester = "AddressesTest";
			bool result = tables.ContainsKey(tester);
			Assert.That(result, Is.True);

			tester = "AddressesTest2";
			result = tables.ContainsKey(tester);
			Assert.That(result, Is.True);

			Table tableItem = (Table)tables["AddressesTest"];
			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(0));

			tableItem = (Table)tables["AddressesTest2"];
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
			Assert.That(count, Is.EqualTo(2));

			foreach (Table table in tables)
			{
				Assert.That(
					table.Name, Is.AnyOf("AddressesTest2", "AddressesTest"));
			}

			Table tableItem = tables[0];
			Assert.That(tableItem.Name, Is.EqualTo("AddressesTest"));

			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(0));

			tableItem = tables[1];
			Assert.That(tableItem.Name, Is.EqualTo("AddressesTest2"));

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

			object tester = "AddressesTest";
			bool result = tables.ContainsKey(tester);
			Assert.That(result, Is.True);

			tester = "AddressesTest2";
			result = tables.ContainsKey(tester);
			Assert.That(result, Is.True);

			Table tableItem = (Table)tables["AddressesTest"];
			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(0));

			tableItem = (Table)tables["AddressesTest2"];
			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(1));
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
