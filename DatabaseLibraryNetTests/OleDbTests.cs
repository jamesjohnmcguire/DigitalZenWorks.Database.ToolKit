// <copyright file="OleDbTests.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.IO;
	using System.Linq;
	using System.Runtime.Versioning;
	using DigitalZenWorks.Common.Utilities;
	using NUnit.Framework;

	/// <summary>
	/// Ole db tests class.
	/// </summary>
	[SupportedOSPlatform("windows")]
	[TestFixture]
	internal sealed class OleDbTests
	{
		/// <summary>
		/// Export schema test.
		/// </summary>
		[Test]
		public static void ExportSchema()
		{
			string databaseFile = GetTestMdbFile();
			string schemaFile = databaseFile + ".sql";

			bool result =
				DataDefinition.ExportSchema(databaseFile, schemaFile);

			Assert.That(result, Is.True);

			string compareSqlFile = GetTestSqlFile();
			string compareText = File.ReadAllText(compareSqlFile);

			string text = File.ReadAllText(schemaFile);
			Assert.That(text, Is.EqualTo(compareText));
		}

		/// <summary>
		/// Get constaints test.
		/// </summary>
		[Test]
		public static void GetConstraints()
		{
			string tableName = "Sections";
			string databaseFile = GetTestMdbFile();

			using OleDbSchema oleDbSchema = new(databaseFile);

			DataTable constraints =
				oleDbSchema.GetConstraints(tableName);

			int count = constraints.Rows.Count;

			Assert.That(count, Is.EqualTo(3));

			bool exists = constraints.Columns.Contains("CONSTRAINT_NAME");
			Assert.That(exists, Is.True);

			exists = constraints.Columns.Contains("TABLE_NAME");
			Assert.That(exists, Is.True);

			exists = constraints.Columns.Contains("COLUMN_NAME");
			Assert.That(exists, Is.True);

			IEnumerable<DataRow> dataRows = constraints.Rows.Cast<DataRow>();
			IEnumerable<string> constraintNameStrings =
				dataRows.Select(row => row["CONSTRAINT_NAME"]?.ToString());
			IEnumerable<string> nonEmptyConstraintNames =
				constraintNameStrings.Where(name => !string.IsNullOrEmpty(name));
			List<string> constraintNames = [.. nonEmptyConstraintNames];

			Assert.That(constraintNames, Contains.Item("SectionsCategories"));
			Assert.That(constraintNames, Contains.Item("SectionsMakers"));

			bool hasPrimaryKeyLikeConstraint = constraintNames.Any(
				name => name.StartsWith("Index_", StringComparison.Ordinal));
			Assert.That(hasPrimaryKeyLikeConstraint, Is.True);

			bool hasCategoriesConstraint =
				constraintNames.Any(name => name.Contains(
					"Categories", StringComparison.Ordinal));
			Assert.That(hasCategoriesConstraint, Is.True);

			foreach (DataRow row in constraints.Rows)
			{
				tableName = row["TABLE_NAME"]?.ToString();
				Assert.That(tableName, Is.EqualTo("Sections"));
			}
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

			Collection<Relationship> relationships =
				DataDefinition.GetRelationships(
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

			Collection<Table> tables = DataDefinition.GetSchema(databaseFile);

			int count = tables.Count;
			Assert.That(count, Is.EqualTo(7));

			foreach (Table table in tables)
			{
				Assert.That(table.Name, Is.AnyOf(
					"Addresses",
					"Categories",
					"Contacts",
					"Makers",
					"Products",
					"Sections",
					"Series"));
			}

			Table tableItem = tables[0];
			Assert.That(tableItem.Name, Is.EqualTo("Addresses"));

			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.Zero);

			tableItem = tables[2];
			Assert.That(tableItem.Name, Is.EqualTo("Contacts"));

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

			Collection<Table> tables = DataDefinition.GetSchema(databaseFile);

			Collection<string> orderedList =
				DataDefinition.OrderTable(tables);

			string tableName = orderedList[0];
			Assert.That(tableName, Is.EqualTo("Addresses"));

			tableName = orderedList[1];
			Assert.That(tableName, Is.EqualTo("Categories"));

			tableName = orderedList[2];
			Assert.That(tableName, Is.EqualTo("Contacts"));

			tableName = orderedList[3];
			Assert.That(tableName, Is.EqualTo("Makers"));

			tableName = orderedList[4];
			Assert.That(tableName, Is.EqualTo("Sections"));

			tableName = orderedList[5];
			Assert.That(tableName, Is.EqualTo("Series"));

			tableName = orderedList[6];
			Assert.That(tableName, Is.EqualTo("Products"));
		}

		private static string GetEmbeddedResourceFile(
			string resource, string extension)
		{
			string fileName = Path.GetTempFileName();

			// A 0 byte sized file is created.  Need to remove it.
			File.Delete(fileName);
			string filePath = Path.ChangeExtension(fileName, extension);

			bool result = FileUtils.CreateFileFromEmbeddedResource(
				resource, filePath);

			Assert.That(result, Is.True);

			result = File.Exists(filePath);
			Assert.That(result, Is.True);

			return filePath;
		}

		private static string GetTestMdbFile()
		{
			string resource =
				"DigitalZenWorks.Database.ToolKit.Tests.Products.Test.accdb";

			string filePath = GetEmbeddedResourceFile(resource, "accdb");

			return filePath;
		}

		private static string GetTestSqlFile()
		{
			string resource = "DigitalZenWorks.Database.ToolKit.Tests." +
				"Products.Access.Test.sql";

			string filePath = GetEmbeddedResourceFile(resource, "sql");

			return filePath;
		}
	}
}
