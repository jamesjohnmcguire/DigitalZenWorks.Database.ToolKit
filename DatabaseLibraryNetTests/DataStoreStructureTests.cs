// <copyright file="DataStoreStructureTests.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Data.SQLite;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using NUnit.Framework;

	/// <summary>
	/// DataStoreStructure tests class.
	/// </summary>
	[TestFixture]
	internal sealed class DataStoreStructureTests : BaseTestsSupport
	{
		/// <summary>
		/// Export schema test.
		/// </summary>
		[Test]
		public void ExportSchema()
		{
			string schemaFile = DataSource + ".sql";

			bool result =
				DataDefinition.ExportSchema(DataSource, schemaFile);

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
		public void GetConstraints()
		{
			string tableName = "Sections";

			using DataStoreStructure schema =
				new (DatabaseType.SQLite, DataSource);

			DataTable constraints =
				schema.GetConstraints(tableName);

			int count = constraints.Rows.Count;

			Assert.That(count, Is.EqualTo(3));

			bool exists = constraints.Columns.Contains("ConstraintName");
			Assert.That(exists, Is.True);

			exists = constraints.Columns.Contains("TableName");
			Assert.That(exists, Is.True);

			exists = constraints.Columns.Contains("ColumnName");
			Assert.That(exists, Is.True);

			IEnumerable<DataRow> dataRows = constraints.Rows.Cast<DataRow>();
			IEnumerable<string> constraintNameStrings =
				dataRows.Select(row => row["ConstraintName"]?.ToString());
			IEnumerable<string> nonEmptyConstraintNames =
				constraintNameStrings.Where(
					name => !string.IsNullOrEmpty(name));
			List<string> constraintNames = [.. nonEmptyConstraintNames];

			// SQLite names foreign key constraints as FK_<table>_<n>_<m>
			Assert.That(constraintNames, Contains.Item("FK_Sections_0_0"));
			Assert.That(constraintNames, Contains.Item("FK_Sections_1_0"));

#if NOT_SQLITE
			bool hasPrimaryKeyLikeConstraint = constraintNames.Any(
				name => name.StartsWith("Index_", StringComparison.Ordinal));
			Assert.That(hasPrimaryKeyLikeConstraint, Is.True);

			bool hasCategoriesConstraint =
				constraintNames.Any(name => name.Contains(
					"Categories", StringComparison.Ordinal));
			Assert.That(hasCategoriesConstraint, Is.True);
#endif

			foreach (DataRow row in constraints.Rows)
			{
				tableName = row["TableName"]?.ToString();
				Assert.That(tableName, Is.EqualTo("Sections"));
			}
		}

		/// <summary>
		/// Get relationships test.
		/// </summary>
		[Test]
		public void GetRelationships()
		{
			const string dependentTableName = "Sections";

			using DataStoreStructure schema =
				new (DatabaseType.SQLite, DataSource);

			Collection<Relationship> relationships =
				schema.GetRelationships(dependentTableName, null);

			int count = relationships.Count;

			Assert.That(count, Is.EqualTo(2));

			Relationship relationship = relationships[0];

			string name = relationship.ParentTable;
			Assert.That(name, Is.EqualTo("Sections"));

			name = relationship.ChildTable;
			Assert.That(name, Is.EqualTo("Makers"));
		}

		/// <summary>
		/// Get schema test.
		/// </summary>
		[Test]
		public void GetSchema()
		{
			using DataStoreStructure schema =
				new (DatabaseType.SQLite, DataSource);

			Collection<Table> tables = schema.GetSchema();

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

			tableItem = tables[6];
			Assert.That(tableItem.Name, Is.EqualTo("Products"));

			count = tableItem.ForeignKeys.Count;
			Assert.That(count, Is.EqualTo(3));
		}

		/// <summary>
		/// GetTableColumns test.
		/// </summary>
		[Test]
		public void GetTableColumns()
		{
			const string tableName = "Addresses";

			using DataStoreStructure schema =
				new (DatabaseType.SQLite, DataSource);

			DataTable table = schema.GetTableColumns(tableName);

			Assert.That(table, Is.Not.Null);

			string name = table.TableName;
			Assert.That(name, Is.EqualTo("Columns"));
		}

		/// <summary>
		/// Order table test.
		/// </summary>
		[Test]
		public void OrderTables()
		{
			Collection<Table> tables = DataDefinition.GetSchema(DataSource);

			Collection<Table> orderedList = DataDefinition.OrderTables(tables);

			Table table = orderedList[0];
			string tableName = table.Name;
			Assert.That(tableName, Is.EqualTo("Addresses"));

			table = orderedList[1];
			tableName = table.Name;
			Assert.That(tableName, Is.EqualTo("Categories"));

			table = orderedList[2];
			tableName = table.Name;
			Assert.That(tableName, Is.EqualTo("Contacts"));

			table = orderedList[3];
			tableName = table.Name;
			Assert.That(tableName, Is.EqualTo("Makers"));

			table = orderedList[4];
			tableName = table.Name;
			Assert.That(tableName, Is.EqualTo("Sections"));

			table = orderedList[5];
			tableName = table.Name;
			Assert.That(tableName, Is.EqualTo("Series"));

			table = orderedList[6];
			tableName = table.Name;
			Assert.That(tableName, Is.EqualTo("Products"));
		}
	}
}
