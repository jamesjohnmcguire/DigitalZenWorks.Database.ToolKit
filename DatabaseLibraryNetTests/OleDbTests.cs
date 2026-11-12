// <copyright file="OleDbTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using NUnit.Framework;

/// <summary>
/// Ole db tests class.
/// </summary>
[SupportedOSPlatform("windows")]
[TestFixture]
internal sealed class OleDbTests : BaseTestsSupport
{
	private string databaseFile;
	private string sqlSchemaFile;

	/// <summary>
	/// Export schema test.
	/// </summary>
	[Test]
	public void ExportSchema()
	{
		string schemaFile = databaseFile + ".sql";

		bool result =
			DataDefinitionOleDb.ExportSchema(databaseFile, schemaFile);

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

		Assert.That(constraintNames, Contains.Item("PrimaryKey"));

		Assert.That(constraintNames, Contains.Item("SectionsCategories"));
		Assert.That(constraintNames, Contains.Item("SectionsMakers"));

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
	/// Direct foreign-key decoding preserves legacy Access metadata.
	/// </summary>
	[Test]
	public void GetSchemaPreservesLegacyRelationshipMetadata()
	{
		using OleDbSchema schema = new(databaseFile);
		Collection<Table> tables = schema.GetSchema();
#pragma warning disable CS0618 // Compare with the retained metadata API.
		Collection<Relationship> relationships = [];
		foreach (Table table in tables)
		{
			relationships = schema.GetRelationships(table.Name, relationships);
		}

		foreach (Table table in tables)
		{
			List<Relationship> expected = relationships.Where(
				item => item.ReferencingTable == table.Name).ToList();
			Assert.That(table.ForeignKeys.Count, Is.EqualTo(expected.Count));
			for (int index = 0; index < expected.Count; index++)
			{
				Relationship relationship = expected[index];
				ForeignKey key = table.ForeignKeys[index];
				Assert.That(key.Name, Is.EqualTo(relationship.Name));
				Assert.That(
					key.ReferencingTable,
					Is.EqualTo(relationship.ReferencingTable));
				Assert.That(
					key.ReferencingColumn,
					Is.EqualTo(relationship.ReferencingColumn));
				Assert.That(
					key.ReferencedTable,
					Is.EqualTo(relationship.ReferencedTable));
				Assert.That(
					key.ReferencedColumn,
					Is.EqualTo(relationship.ReferencedColumn));
				Assert.That(
					key.OnDeleteAction,
					Is.EqualTo(relationship.OnDeleteAction));
				Assert.That(
					key.OnUpdateAction,
					Is.EqualTo(relationship.OnUpdateAction));
			}
		}
#pragma warning restore CS0618
	}

	/// <summary>
	/// A table with outgoing and incoming references returns only its own keys.
	/// </summary>
	[Test]
	public void OwnedForeignKeysExcludeIncomingReferences()
	{
		using OleDbSchema schema = new(databaseFile);
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
		using OleDbSchema schema = new(databaseFile);

		Assert.That(schema.GetForeignKeysOwnedByTable("Makers"), Is.Empty);
	}

	/// <summary>
	/// A self-reference is owned by the table and is returned exactly once.
	/// </summary>
	[Test]
	public void OwnedForeignKeysIncludeSelfReferenceOnce()
	{
		using OleDbSchema schema = new(databaseFile);
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
		using OleDbSchema schema = new(databaseFile);
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
	[Test]
	public void GetRelationships()
	{
		const string dependentTableName = "Addresses";

		using OleDbSchema oleDbSchema = new(databaseFile);

		Collection<Relationship> relationships =
			oleDbSchema.GetRelationships(dependentTableName, null);

		int count = relationships.Count;

		Assert.That(count, Is.EqualTo(1));

		Relationship relationship = relationships[0];

		string name = relationship.ParentTable;
		Assert.That(name, Is.EqualTo("Contacts"));

		name = relationship.ChildTable;
		Assert.That(name, Is.EqualTo("Addresses"));

		Assert.That(relationship.ReferencingTable, Is.EqualTo("Contacts"));
		Assert.That(relationship.ReferencingColumn, Is.EqualTo("addressId"));
		Assert.That(relationship.ReferencedTable, Is.EqualTo("Addresses"));
		Assert.That(relationship.ReferencedColumn, Is.EqualTo("id"));
	}

	/// <summary>
	/// Get schema test.
	/// </summary>
	[Test]
	public void GetSchema()
	{
		Collection<Table> tables =
			DataDefinitionOleDb.GetSchema(databaseFile);

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

		ForeignKey foreignKey = tableItem.ForeignKeys[0];
		Assert.That(foreignKey.ReferencingTable, Is.EqualTo("Contacts"));
		Assert.That(foreignKey.ReferencingColumn, Is.EqualTo("addressId"));
		Assert.That(foreignKey.ReferencedTable, Is.EqualTo("Addresses"));
		Assert.That(foreignKey.ReferencedColumn, Is.EqualTo("id"));
	}

	/// <summary>
	/// GetTableColumns test.
	/// </summary>
	[Test]
	public void GetTableColumns()
	{
		const string tableName = "Addresses";

		using OleDbSchema oleDbSchema = new(databaseFile);

		DataTable table = oleDbSchema.GetTableColumns(tableName);

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

		string fileName = Path.GetTempFileName();

		// A 0 byte sized file is created.  Need to remove it.
		File.Delete(fileName);
		databaseFile = Path.ChangeExtension(fileName, "accdb");

		bool result =
			OleDbHelper.CreateAccessDatabaseFile(databaseFile);
		Assert.That(result, Is.True);

		result = DataDefinitionOleDb.ImportSchema(sqlFile, databaseFile);

		Assert.That(result, Is.True);
	}

	/// <summary>
	/// Order table test.
	/// </summary>
	[Test]
	public void OrderTables()
	{
		Collection<Table> tables =
			DataDefinitionOleDb.GetSchema(databaseFile);

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

	/// <summary>
	/// Gets the database.
	/// </summary>
	protected override void GetDatabase()
	{
		string fileName = Path.GetTempFileName();

		// A 0 byte sized file is created.  Need to remove it.
		File.Delete(fileName);
		databaseFile = Path.ChangeExtension(fileName, "accdb");

		bool result =
			OleDbHelper.CreateAccessDatabaseFile(databaseFile);
		Assert.That(result, Is.True);

		bool exists = File.Exists(databaseFile);
		Assert.That(exists, Is.True);
	}

	/// <summary>
	/// Retrieves the file path to the embedded SQL test file used for unit
	/// testing.
	/// </summary>
	/// <remarks>This method is intended for use in test scenarios where
	/// access to the embedded SQL script is required. The returned file
	/// path can be used to read or execute the test SQL statements.
	/// </remarks>
	/// <returns>A string containing the file path to the embedded SQL test
	/// file. The path will be valid if the resource exists; otherwise, it
	/// may be empty or invalid.</returns>
	protected override string GetTestSqlFile()
	{
		const string resource = "DigitalZenWorks.Database.ToolKit.Tests." +
			"Products.Access.Test.sql";

		string filePath = GetEmbeddedResourceFile(resource, "sql");

		return filePath;
	}

	/// <summary>
	/// Setup the database schema.
	/// </summary>
	protected override void SetupSchema()
	{
		sqlSchemaFile = GetTestSqlFile();

		bool result =
			DataDefinitionOleDb.ImportSchema(sqlSchemaFile, databaseFile);
		Assert.That(result, Is.True);
	}
}
