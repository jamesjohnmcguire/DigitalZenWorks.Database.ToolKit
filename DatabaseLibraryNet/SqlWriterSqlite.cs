/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterSqlite.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

using System;

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
	/// map to NUMERIC rather than REAL, to avoid floating point rounding
	/// on fixed-point/money values.</description></item>
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
#if NET6_0_OR_GREATER
		System.ArgumentNullException.ThrowIfNull(column);
#else
		if (column == null)
		{
			string name = nameof(column);
			throw new System.ArgumentNullException(name);
		}
#endif

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

			// Numeric affinity - fixed point/money types, kept off REAL
			// to avoid floating point rounding on precise values.
			ColumnType.Decimal => " DECIMAL",
			ColumnType.Numeric => " NUMERIC",

			// Numeric affinity - currency/money types, kept off REAL to avoid
			// floating point rounding on precise values. Decimal is the closest
			// SQLite type affinity to the Access Currency type and keeps the
			// values accurately stored as fixed-point rather than floating
			// point.
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
}
