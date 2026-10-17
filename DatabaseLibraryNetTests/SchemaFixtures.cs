/////////////////////////////////////////////////////////////////////////////
// <copyright file="SchemaFixtures.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System.Collections.ObjectModel;
using DigitalZenWorks.Database.ToolKit;

/// <summary>
/// Hand-built Table/Column/ForeignKey fixtures used by SqlWriterSqlite
/// tests. Mirrors the hand-edited SQLite reference schema (see the
/// project's reference .sql file) so generated DDL can be checked against
/// a known-good target, without depending on a live Access/OleDb
/// connection (not available on this platform).
/// </summary>
/// <remarks>
/// GetFullSchema() reproduces every table in the reference file, plus an
/// additional "Orders" table (not present in the original reference file)
/// specifically added to exercise column types the original schema never
/// touches: Currency, DateTime, YesNo/Boolean, and Ole/Blob. Update the
/// reference .sql file to match if "Orders" is kept as a permanent part of
/// the fixture set.
/// </remarks>
public static class SchemaFixtures
{
	/// <summary>
	/// Builds the "Addresses" table: id (AutoNumber PK), stateId (Number,
	/// nullable, DefaultValue "0"), label (Text, not nullable).
	/// </summary>
	/// <returns>The table.</returns>
	public static Table GetAddressesTable()
	{
		Table table = new("Addresses");

		Column column =
			new("id", ColumnType.AutoNumber, 0, false, false, null, 1);
		column.Primary = true;
		table.AddColumn(column);

		column = new("stateId", ColumnType.Number, 0, false, true, "0", 2);
		table.AddColumn(column);

		column = new("label", ColumnType.Text, 0, false, false, null, 3);
		table.AddColumn(column);

		return table;
	}

	/// <summary>
	/// Builds the "Categories" table: id (AutoNumber PK), name (Text, not
	/// nullable), parentId (Number, nullable, self-referencing FK).
	/// </summary>
	/// <returns>The table.</returns>
	public static Table GetCategoriesTable()
	{
		Table table = new("Categories");

		Column column =
			new("id", ColumnType.AutoNumber, 0, false, false, null, 1);
		column.Primary = true;
		table.AddColumn(column);

		column = new("name", ColumnType.Text, 0, false, false, null, 2);
		table.AddColumn(column);

		column = new("parentId", ColumnType.Number, 0, false, true, null, 3);
		table.AddColumn(column);

		table.ForeignKeys.Add(new ForeignKey(
			"FK_Categories_0_0",
			"parentId",
			"Categories",
			"id",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade));

		return table;
	}

	/// <summary>
	/// Builds the "Contacts" table: id (AutoNumber PK), addressId (Number,
	/// nullable, FK to Addresses), label (Text, not nullable).
	/// </summary>
	/// <returns>The table.</returns>
	public static Table GetContactsTable()
	{
		Table table = new("Contacts");

		Column column =
			new("id", ColumnType.AutoNumber, 0, false, false, null, 1);
		column.Primary = true;
		table.AddColumn(column);

		column = new("addressId", ColumnType.Number, 0, false, true, null, 2);
		table.AddColumn(column);

		column = new("label", ColumnType.Text, 0, false, false, null, 3);
		table.AddColumn(column);

		table.ForeignKeys.Add(new ForeignKey(
			"FK_Contacts_0_0",
			"addressId",
			"Addresses",
			"id",
			ConstraintAction.Cascade,
			ConstraintAction.Cascade));

		return table;
	}

	/// <summary>
	/// Builds the "Makers" table: id (AutoNumber PK), name (Text, not
	/// nullable).
	/// </summary>
	/// <returns>The table.</returns>
	public static Table GetMakersTable()
	{
		Table table = new("Makers");

		Column column =
			new("id", ColumnType.AutoNumber, 0, false, false, null, 1);
		column.Primary = true;
		table.AddColumn(column);

		column = new("name", ColumnType.Text, 0, false, false, null, 2);
		table.AddColumn(column);

		return table;
	}

	/// <summary>
	/// Builds the "Sections" table: id (AutoNumber PK), categoryId
	/// (Number, nullable, FK to Categories), makerId (Number, nullable,
	/// FK to Makers), label (Text, nullable).
	/// </summary>
	/// <returns>The table.</returns>
	public static Table GetSectionsTable()
	{
		Table table = new("Sections");

		Column column =
			new("id", ColumnType.AutoNumber, 0, false, false, null, 1);
		column.Primary = true;
		table.AddColumn(column);

		column =
			new("categoryId", ColumnType.Number, 0, false, true, null, 2);
		table.AddColumn(column);

		column =
			new("makerId", ColumnType.Number, 0, false, true, null, 3);
		table.AddColumn(column);

		column =
			new("label", ColumnType.Text, 0, false, true, null, 4);
		table.AddColumn(column);

		table.ForeignKeys.Add(new ForeignKey(
			"FK_Sections_1_0",
			"categoryId",
			"Categories",
			"id",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade));

		table.ForeignKeys.Add(new ForeignKey(
			"FK_Sections_0_0",
			"makerId",
			"Makers",
			"id",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade));

		return table;
	}

	/// <summary>
	/// Builds the "Series" table: id (AutoNumber PK), makerId (Number,
	/// nullable, FK to Makers), label (Text, not nullable).
	/// </summary>
	/// <returns>The table.</returns>
	public static Table GetSeriesTable()
	{
		Table table = new("Series");

		Column column =
			new("id", ColumnType.AutoNumber, 0, false, false, null, 1);
		column.Primary = true;
		table.AddColumn(column);

		column =
			new("makerId", ColumnType.Number, 0, false, true, null, 2);
		table.AddColumn(column);

		column =
			new("label", ColumnType.Text, 0, false, false, null, 3);
		table.AddColumn(column);

		table.ForeignKeys.Add(new ForeignKey(
			"FK_Series_0_0",
			"makerId",
			"Makers",
			"id",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade));

		return table;
	}

	/// <summary>
	/// Builds the "Products" table: id (AutoNumber PK), makerId, sectionId,
	/// seriesId (all Number, nullable, FKs), label (Text, nullable).
	/// </summary>
	/// <returns>The table.</returns>
	public static Table GetProductsTable()
	{
		Table table = new("Products");

		Column column =
			new("id", ColumnType.AutoNumber, 0, false, false, null, 1);
		column.Primary = true;
		table.AddColumn(column);

		column =
			new("makerId", ColumnType.Number, 0, false, true, null, 2);
		table.AddColumn(column);

		column =
			new("sectionId", ColumnType.Number, 0, false, true, null, 3);
		table.AddColumn(column);

		column =
			new("seriesId", ColumnType.Number, 0, false, true, null, 4);
		table.AddColumn(column);

		column =
			new("label", ColumnType.Text, 0, false, true, null, 5);
		table.AddColumn(column);

		table.ForeignKeys.Add(new ForeignKey(
			"FK_Products_2_0",
			"makerId",
			"Makers",
			"id",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade));

		table.ForeignKeys.Add(new ForeignKey(
			"FK_Products_1_0",
			"sectionId",
			"Sections",
			"id",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade));

		table.ForeignKeys.Add(new ForeignKey(
			"FK_Products_0_0",
			"seriesId",
			"Series",
			"id",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade));

		return table;
	}

	/// <summary>
	/// Builds the "Orders" table. Not present in the original reference
	/// file — added specifically to exercise Currency, DateTime,
	/// YesNo/Boolean, and Ole/Blob column types, which the original
	/// six-table schema never touches.
	/// </summary>
	/// <returns>The table.</returns>
	public static Table GetOrdersTable()
	{
		Table table = new("Orders");

		Column column =
			new("id", ColumnType.AutoNumber, 0, false, false, null, 1);
		column.Primary = true;
		table.AddColumn(column);

		column =
			new("productId", ColumnType.Number, 0, false, true, null, 2);
		table.AddColumn(column);

		column =
			new("price", ColumnType.Currency, 0, false, false, null, 3);
		table.AddColumn(column);

		column =
			new("orderDate", ColumnType.DateTime, 0, false, false, null, 4);
		table.AddColumn(column);

		column =
			new("isShipped", ColumnType.YesNo, 0, false, false, "0", 5);
		table.AddColumn(column);

		column =
			new("photo", ColumnType.Ole, 0, false, true, null, 6);
		table.AddColumn(column);

		column =
			new("notes", ColumnType.Memo, 0, false, true, null, 7);
		table.AddColumn(column);

		table.ForeignKeys.Add(new ForeignKey(
			"FK_Orders_0_0",
			"productId",
			"Products",
			"id",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade));

		return table;
	}

	/// <summary>
	/// Returns the full schema (all tables) in FK-dependency-safe order,
	/// matching the reference .sql file's table ordering.
	/// </summary>
	/// <returns>The collection of tables.</returns>
	public static Collection<Table> GetFullSchema()
	{
		Collection<Table> tables = [];

		tables.Add(GetAddressesTable());
		tables.Add(GetCategoriesTable());
		tables.Add(GetContactsTable());
		tables.Add(GetMakersTable());
		tables.Add(GetSectionsTable());
		tables.Add(GetSeriesTable());
		tables.Add(GetProductsTable());
		tables.Add(GetOrdersTable());

		return tables;
	}
}
