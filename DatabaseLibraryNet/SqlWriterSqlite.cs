/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterSqlite.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

/// <summary>
/// SQL writer helper for SQLite 3.x class.
/// </summary>
/// <remarks>
/// SQLite uses a small set of type affinities (TEXT, NUMERIC, INTEGER,
/// REAL, BLOB) rather than the wide, precise type systems of Access,
/// MySQL, or SQL Server. This class maps every <see cref="ColumnType"/>
/// value onto the closest matching affinity. The base <see cref="SqlWriter"/>
/// class already produces SQLite-compatible structural output (double
/// quoted identifiers, inline PRIMARY KEY AUTOINCREMENT), so this class
/// only needs to override the type-mapping step.
/// </remarks>
public class SqlWriterSqlite : SqlWriter
{
	/// <summary>
	/// Generates a SQLite CREATE TABLE statement for the specified table.
	/// </summary>
	/// <param name="table">The source table structure.</param>
	/// <param name="isLast">Whether this is the last generated table.</param>
	/// <returns>The executable SQLite CREATE TABLE statement.</returns>
	public override string GetTableCreateStatement(
		Table table, bool isLast = false)
	{
		ArgumentGuard.ThrowIfNull(table, nameof(table));

		SortedList<int, Column> columns = GetOrdinalSortedColumns(table);
		List<Column> primaryColumns = [];

		foreach (KeyValuePair<int, Column> entry in columns)
		{
			if (entry.Value.Primary)
			{
				primaryColumns.Add(entry.Value);
			}
		}

		List<string> definitions = [];

		foreach (KeyValuePair<int, Column> entry in columns)
		{
			string column =
				GetSqliteColumnSql(entry.Value, primaryColumns.Count);
			definitions.Add(column);
		}

		if (primaryColumns.Count > 1)
		{
			string key = GetCompositePrimaryKeySql(primaryColumns);
			definitions.Add(key);
		}

		Collection<ForeignKey> foreignKeys = table.ForeignKeys;

		for (int index = foreignKeys.Count - 1; index >= 0; index--)
		{
			string foreignKey = GetForeignKeySql(foreignKeys[index], true);
			definitions.Add(foreignKey);
		}

		string sql = string.Format(
			CultureInfo.InvariantCulture,
			"CREATE TABLE {0}{1}({1}",
			QuoteIdentifier(table.Name),
			DefaultLineEnding);

		sql += string.Join("," + DefaultLineEnding, definitions);
		sql += DefaultLineEnding + ");";

		if (isLast == false)
		{
			sql += DefaultLineEnding;
		}

		return sql;
	}

	/// <summary>
	/// Returns the SQLite type affinity string corresponding to the
	/// specified column's <see cref="ColumnType"/>.
	/// </summary>
	/// <remarks>
	/// Two mappings here are deliberate judgment calls rather than
	/// obvious translations:
	/// <list type="bullet">
	/// <item><description><see cref="ColumnType.Currency"/>,
	/// <see cref="ColumnType.Money"/>, <see cref="ColumnType.SmallMoney"/>,
	/// <see cref="ColumnType.Decimal"/>, and <see cref="ColumnType.Numeric"/>
	/// use DECIMAL or NUMERIC declarations to preserve their intent,
	/// without guaranteeing exact decimal storage in SQLite.</description></item>
	/// <item><description><see cref="ColumnType.AutoNumber"/> and
	/// <see cref="ColumnType.Identity"/> map to INTEGER; combined with the
	/// base class's existing "PRIMARY KEY AUTOINCREMENT" clause (added
	/// separately, based on <c>column.Primary</c>), this produces SQLite's
	/// rowid-alias auto-increment behavior.</description></item>
	/// </list>
	/// <see cref="ColumnType.SqlVariant"/> and
	/// <see cref="ColumnType.LookupWizard"/> have no reasonable SQLite
	/// equivalent and are stored opaquely as BLOB rather than guessed at.
	/// </remarks>
	/// <param name="column">The column for which to generate the SQL type
	/// declaration.</param>
	/// <returns>A string representing the SQLite type affinity for the
	/// column.</returns>
	protected override string GetColumnTypeText(Column column)
	{
		ArgumentGuard.ThrowIfNull(column, nameof(column));

		string columnType = column.ColumnType switch
		{
			// Text affinity - character/string types.
			ColumnType.Char => " TEXT",
			ColumnType.Enum => " TEXT",
			ColumnType.Hyperlink => " TEXT",
			ColumnType.LongText => " TEXT",
			ColumnType.LongVarChar => " TEXT",
			ColumnType.MediumText => " TEXT",
			ColumnType.Memo => " TEXT",
			ColumnType.NChar => " TEXT",
			ColumnType.NText => " TEXT",
			ColumnType.NVarChar => " TEXT",
			ColumnType.Set => " TEXT",
			ColumnType.String => " TEXT",
			ColumnType.Text => " TEXT",
			ColumnType.TinyText => " TEXT",
			ColumnType.UniqueIdentifier => " TEXT",
			ColumnType.VarChar => " TEXT",
			ColumnType.Xml => " TEXT",

			// Text affinity - date/time types, stored as ISO-8601 text
			// per SQLite's own recommended date/time convention.
			ColumnType.Date => " TEXT",
			ColumnType.DateTime => " TEXT",
			ColumnType.DateTime2 => " TEXT",
			ColumnType.DateTimeOffset => " TEXT",
			ColumnType.SmallDateTime => " TEXT",
			ColumnType.Time => " TEXT",
			ColumnType.Timestamp => " TEXT",
			ColumnType.Year => " TEXT",

			// Integer affinity - whole-number types.
			ColumnType.AutoNumber => " INTEGER",
			ColumnType.BigInt => " INTEGER",
			ColumnType.Byte => " INTEGER",
			ColumnType.Identity => " INTEGER",
			ColumnType.Int => " INTEGER",
			ColumnType.Integer => " INTEGER",
			ColumnType.Long => " INTEGER",
			ColumnType.MediumInt => " INTEGER",
			ColumnType.Number => " INTEGER",
			ColumnType.SmallInt => " INTEGER",
			ColumnType.TinyInt => " INTEGER",

			// Integer affinity - boolean types (SQLite has no native
			// boolean; stored as 0/1).
			ColumnType.Bit => " INTEGER",
			ColumnType.Boolean => " INTEGER",
			ColumnType.YesNo => " INTEGER",

			// Ultimately a LookupWizard is just a mechanism to store a foreign
			// key reference, so store as INTEGER.
			ColumnType.LookupWizard => " INTEGER",

			// Real affinity - floating point types.
			ColumnType.Float => " REAL",
			ColumnType.Double => " REAL",
			ColumnType.Single => " REAL",
			ColumnType.Real => " REAL",

			// Preserve exact-numeric type names; both have NUMERIC affinity.
			ColumnType.Decimal => " DECIMAL",
			ColumnType.Numeric => " NUMERIC",

			// DECIMAL expresses monetary intent. SQLite may still store REAL.
			ColumnType.Currency => " DECIMAL",
			ColumnType.Money => " DECIMAL",
			ColumnType.SmallMoney => " DECIMAL",

			// Blob affinity - binary/opaque types.
			ColumnType.Binary => " BLOB",
			ColumnType.Blob => " BLOB",
			ColumnType.Image => " BLOB",
			ColumnType.JavaObject => " BLOB",
			ColumnType.LongVarBinary => " BLOB",
			ColumnType.LongBlob => " BLOB",
			ColumnType.MediumBlob => " BLOB",
			ColumnType.Ole => " BLOB",
			ColumnType.OleObject => " BLOB",
			ColumnType.VarBinary => " BLOB",

			// No reasonable SQLite equivalent - stored opaquely rather
			// than guessed at.
			ColumnType.SqlVariant => " BLOB",

			// Not real persisted column types in practice; safe non-empty
			// fallback so this switch never silently degrades to blank
			// output the way the base implementation does today.
			ColumnType.Cursor => " TEXT",
			ColumnType.Other => " TEXT",
			ColumnType.Table => " TEXT",
			ColumnType.Unknown => " TEXT",

			_ => " TEXT",
		};

		return columnType;
	}

	/// <inheritdoc />
	protected override string GetForeignKeySql(
		ForeignKey foreignKey, bool isLast)
	{
		ArgumentGuard.ThrowIfNull(foreignKey, nameof(foreignKey));

		string keyName = QuoteIdentifier(foreignKey.Name);
		string referencingColumn =
			QuoteIdentifier(foreignKey.ReferencingColumn);
		string referencedTable = QuoteIdentifier(foreignKey.ReferencedTable);
		string referencedColumn = QuoteIdentifier(foreignKey.ReferencedColumn);

		string sql = string.Format(
			CultureInfo.InvariantCulture,
			"\tCONSTRAINT {0} FOREIGN KEY ({1}) REFERENCES {2}({3})",
			keyName,
			referencingColumn,
			referencedTable,
			referencedColumn);

		sql += GetContstraintActionText("DELETE", foreignKey.OnDeleteAction);
		sql += GetContstraintActionText("UPDATE", foreignKey.OnUpdateAction);

		if (isLast == false)
		{
			sql += "," + DefaultLineEnding;
		}

		return sql;
	}

	/// <summary>
	/// Returns the SQLite-specific identity keyword text for the specified.
	/// </summary>
	/// <param name="column">The column for which to generate the identity
	/// keyword. Cannot be null.</param>
	/// <returns>A string representing the SQLite identity keyword for the
	/// column.</returns>
	/// <remarks>SQLite has no IDENTITY keyword; auto-increment is expressed
	/// entirely through "INTEGER PRIMARY KEY AUTOINCREMENT" which GetColumnSql
	/// already appends separately.</remarks>
	protected override string GetIdentityKeywordText(Column column)
	{
		return string.Empty;
	}

	private static string GetCompositePrimaryKeySql(
		IList<Column> primaryColumns)
	{
		List<string> names = [];

		foreach (Column column in primaryColumns)
		{
			string identifier = QuoteIdentifier(column.Name);
			names.Add(identifier);
		}

		string columns = string.Join(", ", names);
		string key = "\tPRIMARY KEY (" + columns + ")";

		return key;
	}

	private static string GetSqliteDefaultValue(string defaultValue)
	{
		string value = defaultValue.Trim();

		// A leading '=' if present, which indicates an Access expression.
		bool isAccessExpression = value.Length > 0 && value[0] == '=';

		if (isAccessExpression)
		{
			value = value.Substring(1);
			value = value.Trim();
		}

		if (value.Equals("Now()", StringComparison.OrdinalIgnoreCase))
		{
			value = "CURRENT_TIMESTAMP";
		}
		else if (value.Equals("Date()", StringComparison.OrdinalIgnoreCase) ||
			value.Equals("Date$()", StringComparison.OrdinalIgnoreCase))
		{
			value = "CURRENT_DATE";
		}
		else if (value.Equals("Time()", StringComparison.OrdinalIgnoreCase) ||
			value.Equals("Time$()", StringComparison.OrdinalIgnoreCase))
		{
			value = "CURRENT_TIME";
		}
		else if (isAccessExpression == true &&
			!string.IsNullOrWhiteSpace(value))
		{
			if (value.Length < 2 || value[0] != '(' ||
				value[value.Length - 1] != ')')
			{
				value = "(" + value + ")";
			}
		}

		return value;
	}

	private static string QuoteIdentifier(string identifier)
	{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		string escaped =
			identifier.Replace("\"", "\"\"", StringComparison.Ordinal);
#else
		string escaped = identifier.Replace("\"", "\"\"");
#endif
		escaped = "\"" + escaped + "\"";

		return escaped;
	}

	private string GetSqliteColumnSql(Column column, int primaryKeyCount)
	{
		bool isAutomatic = column.ColumnType == ColumnType.AutoNumber ||
			column.ColumnType == ColumnType.Identity;

		string columnType = GetColumnTypeText(column);

		if (column.Primary && primaryKeyCount == 1 && isAutomatic == false &&
			columnType.Equals(" INTEGER", StringComparison.Ordinal))
		{
			// Need to use " INT" instead of " INTEGER" because SQLite treats
			// "INTEGER PRIMARY KEY" specially. It would aliases rowid and
			// generate values.
			columnType = " INT";
		}

		string sql = "\t" + QuoteIdentifier(column.Name);
		sql += columnType;

		if (column.Unique)
		{
			sql += " UNIQUE";
		}

		if (column.Primary)
		{
			if (primaryKeyCount == 1 && isAutomatic)
			{
				sql += " PRIMARY KEY AUTOINCREMENT";
			}
			else if (primaryKeyCount == 1)
			{
				sql += " NOT NULL PRIMARY KEY";
			}
			else
			{
				sql += " NOT NULL";
			}
		}
		else if (!column.Nullable)
		{
			sql += " NOT NULL";
		}

		if (!string.IsNullOrWhiteSpace(column.DefaultValue))
		{
			string defaultValue = GetSqliteDefaultValue(column.DefaultValue);

			if (!string.IsNullOrWhiteSpace(defaultValue))
			{
				sql += " DEFAULT " + defaultValue;
			}
		}

		return sql;
	}
}
