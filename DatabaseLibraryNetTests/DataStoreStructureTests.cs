// <copyright file="DataStoreStructureTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
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
	/// Schema reading preserves the decimal declaration of monetary data.
	/// </summary>
	[Test]
	public void PriceRetainsDecimalType()
	{
		using DataStoreStructure schema =
			new(DatabaseType.SQLite, DataSource);
		Table table = schema.GetTable("Orders");

		Assert.That(
			table.Columns["price"].ColumnType,
			Is.EqualTo(ColumnType.Decimal));
	}

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
			new(DatabaseType.SQLite, DataSource);

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
	/// A table with outgoing and incoming references returns only its own keys.
	/// </summary>
	[Test]
	public void OwnedForeignKeysExcludeIncomingReferences()
	{
		using DataStoreStructure schema = new(DatabaseType.SQLite, DataSource);
		Collection<ForeignKey> keys =
			schema.GetForeignKeysOwnedByTable("Sections");

		string[] expectedTables = ["Categories", "Makers"];
		string[] expectedColumns = ["categoryId", "makerId"];
		Assert.That(keys.Count, Is.EqualTo(2));
		Assert.That(
			keys.Select(key => key.ReferencedTable),
			Is.EquivalentTo(expectedTables));
		Assert.That(
			keys.Select(key => key.ReferencingColumn),
			Is.EquivalentTo(expectedColumns));
		foreach (ForeignKey key in keys)
		{
			Assert.That(key.Name, Is.Not.Empty);
			Assert.That(key.ReferencingTable, Is.EqualTo("Sections"));
			Assert.That(key.ReferencedColumn, Is.EqualTo("id"));
			Assert.That(
				key.OnDeleteAction, Is.EqualTo(ConstraintAction.SetNull));
			Assert.That(
				key.OnUpdateAction, Is.EqualTo(ConstraintAction.Cascade));
		}
	}

	/// <summary>
	/// Being referenced by other tables does not imply ownership of a key.
	/// </summary>
	[Test]
	public void OwnedForeignKeysReturnEmptyForReferencedOnlyTable()
	{
		using DataStoreStructure schema = new(DatabaseType.SQLite, DataSource);

		Assert.That(schema.GetForeignKeysOwnedByTable("Makers"), Is.Empty);
	}

	/// <summary>
	/// A self-reference is owned by the table and is returned exactly once.
	/// </summary>
	[Test]
	public void OwnedForeignKeysIncludeSelfReferenceOnce()
	{
		using DataStoreStructure schema = new(DatabaseType.SQLite, DataSource);
		Collection<ForeignKey> keys =
			schema.GetForeignKeysOwnedByTable("Categories");

		Assert.That(keys.Count, Is.EqualTo(1));
		Assert.That(keys[0].ReferencingTable, Is.EqualTo("Categories"));
		Assert.That(keys[0].ReferencingColumn, Is.EqualTo("parentId"));
		Assert.That(keys[0].ReferencedTable, Is.EqualTo("Categories"));
		Assert.That(keys[0].ReferencedColumn, Is.EqualTo("id"));
	}

	/// <summary>
	/// Queries return independent collections, without accumulating results.
	/// </summary>
	[Test]
	public void OwnedForeignKeysReturnIndependentCollections()
	{
		using DataStoreStructure schema = new(DatabaseType.SQLite, DataSource);
		Collection<ForeignKey> first =
			schema.GetForeignKeysOwnedByTable("Sections");
		Collection<ForeignKey> second =
			schema.GetForeignKeysOwnedByTable("Sections");

		Assert.That(second, Is.Not.SameAs(first));
		first.Clear();
		Assert.That(second.Count, Is.EqualTo(2));
	}

	/// <summary>
	/// Get relationships test.
	/// </summary>
#pragma warning disable CS0618 // Verify the legacy relationship query.
	[Test]
	public void GetRelationships()
	{
		const string dependentTableName = "Sections";

		using DataStoreStructure schema =
			new(DatabaseType.SQLite, DataSource);

		Collection<Relationship> relationships =
			schema.GetRelationships(dependentTableName, null);

		int count = relationships.Count;

		Assert.That(count, Is.EqualTo(2));

		Relationship relationship = relationships[0];

		string name = relationship.ParentTable;
		Assert.That(name, Is.EqualTo("Sections"));

		name = relationship.ChildTable;
		Assert.That(name, Is.EqualTo("Makers"));

		Assert.That(relationship.ReferencingTable, Is.EqualTo("Sections"));
		Assert.That(relationship.ReferencingColumn, Is.EqualTo("makerId"));
		Assert.That(relationship.ReferencedTable, Is.EqualTo("Makers"));
		Assert.That(relationship.ReferencedColumn, Is.EqualTo("id"));
	}
#pragma warning restore CS0618

	/// <summary>
	/// Get schema test.
	/// </summary>
	[Test]
	public void GetSchema()
	{
		using DataStoreStructure schema =
			new(DatabaseType.SQLite, DataSource);

		Collection<Table> tables = schema.GetSchema();

		int count = tables.Count;
		Assert.That(count, Is.EqualTo(8));

		foreach (Table table in tables)
		{
			Assert.That(table.Name, Is.AnyOf(
				"Addresses",
				"Categories",
				"Contacts",
				"Makers",
				"Orders",
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

		foreach (ForeignKey foreignKey in tableItem.ForeignKeys)
		{
			Assert.That(foreignKey.ReferencingTable, Is.EqualTo("Products"));
			Assert.That(foreignKey.ReferencedColumn, Is.EqualTo("id"));
			Assert.That(
				foreignKey.ReferencingColumn,
				Is.AnyOf("makerId", "sectionId", "seriesId"));
		}
	}

	/// <summary>
	/// GetTableColumns test.
	/// </summary>
	[Test]
	public void GetTableColumns()
	{
		const string tableName = "Addresses";

		using DataStoreStructure schema =
			new(DatabaseType.SQLite, DataSource);

		DataTable table = schema.GetTableColumns(tableName);

		Assert.That(table, Is.Not.Null);

		string name = table.TableName;
		Assert.That(name, Is.EqualTo("Columns"));
	}

	/// <summary>
	/// Import schema test.
	/// </summary>
	[Test]
	public void ImportSchema()
	{
		string sqlFile = GetTestSqlFile();

		string newDataSource = GetTestDatabasePath();

		SQLiteConnection.CreateFile(newDataSource);

		bool result =
			DataDefinition.ImportSchema(sqlFile, newDataSource);

		Assert.That(result, Is.True);
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
