/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterTypeTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit.Tests;

using NUnit.Framework;

/// <summary>
/// Verifies type declarations through the public table-writing API.
/// </summary>
[TestFixture]
internal sealed class SqlWriterTypeTests
{
	/// <summary>
	/// Checks generic declarations and the Access-specific differences.
	/// </summary>
	/// <param name="type">The source column type.</param>
	/// <param name="generic">The expected generic declaration.</param>
	/// <param name="access">The expected Access declaration.</param>
	[TestCase(ColumnType.Numeric, "NUMERIC", "NUMERIC")]
	[TestCase(ColumnType.Decimal, "DECIMAL", "DECIMAL")]
	[TestCase(ColumnType.Number, "INTEGER", "INTEGER")]
	[TestCase(ColumnType.BigInt, "BIGINT", "BIGINT")]
	[TestCase(ColumnType.SmallInt, "SMALLINT", "SMALLINT")]
	[TestCase(ColumnType.Double, "DOUBLE PRECISION", "DOUBLE")]
	[TestCase(ColumnType.Real, "REAL", "SINGLE")]
	[TestCase(ColumnType.Float, "FLOAT", "DOUBLE")]
	[TestCase(ColumnType.String, "VARCHAR(42)", "VARCHAR(42)")]
	[TestCase(ColumnType.Char, "CHAR(42)", "CHAR(42)")]
	[TestCase(ColumnType.NVarChar, "NVARCHAR(42)", "NVARCHAR(42)")]
	[TestCase(ColumnType.Binary, "BINARY(42)", "BINARY(42)")]
	[TestCase(ColumnType.VarBinary, "VARBINARY(42)", "VARBINARY(42)")]
	[TestCase(ColumnType.Date, "DATE", "DATETIME")]
	[TestCase(ColumnType.Time, "TIME", "DATETIME")]
	[TestCase(ColumnType.DateTime, "TIMESTAMP", "DATETIME")]
	[TestCase(ColumnType.Timestamp, "TIMESTAMP", "DATETIME")]
	[TestCase(ColumnType.Currency, "DECIMAL(19,4)", "CURRENCY")]
	[TestCase(ColumnType.Memo, "TEXT", "MEMO")]
	[TestCase(ColumnType.Ole, "BLOB", "OLEOBJECT")]
	[TestCase(ColumnType.OleObject, "BLOB", "OLEOBJECT")]
	[TestCase(ColumnType.YesNo, "BOOLEAN", "BIT")]
	[TestCase(ColumnType.Boolean, "BOOLEAN", "BIT")]
	[TestCase(ColumnType.Byte, "SMALLINT", "BYTE")]
	public void TableUsesDialectType(
		ColumnType type, string generic, string access)
	{
		Table table = new("Types");
		Column column = new("value", type, 42, false, true, null, 1);
		table.AddColumn(column);

		string genericSql = new SqlWriter().GetTableCreateStatement(table);
		string accessSql = new SqlWriterOleDb().GetTableCreateStatement(table);

		Assert.That(genericSql, Does.Contain("\"value\" " + generic));
		Assert.That(accessSql, Does.Contain("[value] " + access));
	}
}
