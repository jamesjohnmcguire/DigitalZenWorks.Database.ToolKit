/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterCompatibilityTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using NUnit.Framework;

/// <summary>
/// Protects SQL text and subclass hooks while consolidating writers.
/// </summary>
[TestFixture]
internal sealed class SqlWriterCompatibilityTests
{
	/// <summary>
	/// Current and obsolete column methods keep their different constraints.
	/// </summary>
	/// <param name="automatic">Whether the column generates values.</param>
	/// <param name="primary">Whether the column is a primary key.</param>
	/// <param name="generic">The current generic constraints.</param>
	/// <param name="legacy">The obsolete generic constraints.</param>
	/// <param name="access">The current Access constraints.</param>
	/// <param name="legacyAccess">The obsolete Access constraints.</param>
	[TestCase(
		false,
		false,
		" NOT NULL",
		" NOT NULL",
		" NOT NULL",
		" NOT NULL")]
	[TestCase(
		false,
		true,
		" PRIMARY KEY AUTOINCREMENT",
		" NOT NULL",
		" NOT NULL IDENTITY",
		" NOT NULL")]
	[TestCase(
		true,
		false,
		" IDENTITY NOT NULL",
		" NOT NULL IDENTITY",
		" NOT NULL IDENTITY",
		" NOT NULL IDENTITY")]
	[TestCase(
		true,
		true,
		" IDENTITY PRIMARY KEY AUTOINCREMENT",
		" NOT NULL IDENTITY",
		" NOT NULL IDENTITY",
		" NOT NULL IDENTITY")]
	public void ColumnMethodsPreserveExactSql(
		bool automatic,
		bool primary,
		string generic,
		string legacy,
		string access,
		string legacyAccess)
	{
		Column column = new("id", ColumnType.Number, 0, primary, false, "7", 1);
		column.Primary = primary;
		if (automatic)
		{
			column.ColumnType = ColumnType.AutoNumber;
		}

		column.Unique = true;
		WriterProbe writer = new();
		AccessWriterProbe accessWriter = new();
		Assert.That(
			writer.ColumnSql(column, false),
			Is.EqualTo("\t\"id\" INTEGER UNIQUE" + generic + " DEFAULT 7,\n"));
		Assert.That(
			writer.LegacyColumnSql(column),
			Is.EqualTo("\t\"id\" INTEGER UNIQUE" + legacy + " DEFAULT 7,\n"));
		Assert.That(
			accessWriter.ColumnSql(column, false),
			Is.EqualTo("\t[id] INTEGER UNIQUE" + access + " DEFAULT 7," +
				Environment.NewLine));
		Assert.That(
			accessWriter.LegacyColumnSql(column),
			Is.EqualTo("\t[id] INTEGER UNIQUE" + legacyAccess + " DEFAULT 7," +
				Environment.NewLine));
		Assert.That(
			writer.ColumnSql(column, true),
			Is.EqualTo("\t\"id\" INTEGER UNIQUE" + generic + " DEFAULT 7\n"));
		Assert.That(
			accessWriter.ColumnSql(column, true),
			Is.EqualTo("\t[id] INTEGER UNIQUE" + access + " DEFAULT 7" +
				Environment.NewLine));
	}

	/// <summary>
	/// Null columns retain the parameter name in both writer entry points.
	/// </summary>
	[Test]
	public void NullColumnsRetainArgumentExceptions()
	{
		WriterProbe writer = new();
		AccessWriterProbe accessWriter = new();
		Assert.That(
			() => writer.ColumnSql(null, false),
			Throws.ArgumentNullException.With.Property("ParamName").EqualTo("column"));
		Assert.That(
			() => writer.LegacyColumnSql(null),
			Throws.ArgumentNullException.With.Property("ParamName").EqualTo("column"));
		Assert.That(
			() => accessWriter.ColumnSql(null, false),
			Throws.ArgumentNullException.With.Property("ParamName").EqualTo("column"));
		Assert.That(
			() => accessWriter.LegacyColumnSql(null),
			Throws.ArgumentNullException.With.Property("ParamName").EqualTo("column"));
	}

	/// <summary>
	/// Obsolete methods still use type hooks without calling column overrides.
	/// </summary>
	[Test]
	public void LegacyMethodsPreserveSubclassDispatch()
	{
		Column column = new("id", ColumnType.AutoNumber, 0, false, true, null, 1);
		column.Primary = true;
		CustomizedWriter writer = new();
		CustomizedAccessWriter accessWriter = new();
		Assert.That(
			writer.LegacyColumnSql(column),
			Is.EqualTo("\t\"id\" CUSTOM GENERATED,\n"));
		Assert.That(
			accessWriter.LegacyColumnSql(column),
			Is.EqualTo("\t[id] CUSTOM IDENTITY," + Environment.NewLine));
	}

	/// <summary>
	/// Actions, identifier syntax and punctuation remain dialect-specific.
	/// </summary>
	/// <param name="action">The requested action.</param>
	/// <param name="suffix">The expected action clauses.</param>
	[TestCase(ConstraintAction.Cascade, " ON DELETE CASCADE ON UPDATE CASCADE")]
	[TestCase(ConstraintAction.SetNull, " ON DELETE SET NULL ON UPDATE SET NULL")]
	[TestCase(ConstraintAction.NoAction, "")]
	[TestCase((ConstraintAction)999, "")]
	public void ForeignKeySqlPreservesDialectAndActions(
		ConstraintAction action, string suffix)
	{
		ForeignKey key = ForeignKey.Create(
			"FK", "Products", "id", "Orders", "productId", action, action);
		WriterProbe writer = new();
		AccessWriterProbe accessWriter = new();
		SqliteWriterProbe sqliteWriter = new();
		const string generic =
			"\tCONSTRAINT \"FK\" FOREIGN KEY(\"productId\")" +
			" REFERENCES \"Products\"(\"id\")";
		const string access =
			"\tCONSTRAINT [FK] FOREIGN KEY ([productId]) REFERENCES [Products] ([id])";
		const string sqlite =
			"\tCONSTRAINT \"FK\" FOREIGN KEY (\"productId\")" +
			" REFERENCES \"Products\"(\"id\")";
		Assert.That(writer.ForeignKeySql(key, true), Is.EqualTo(generic + suffix));
		Assert.That(
			writer.ForeignKeySql(key, false), Is.EqualTo(generic + suffix + ",\n"));
		Assert.That(
			accessWriter.ForeignKeySql(key, true), Is.EqualTo(access + suffix));
		Assert.That(
			accessWriter.ForeignKeySql(key, false),
			Is.EqualTo(access + suffix + "," + Environment.NewLine));
		Assert.That(
			sqliteWriter.ForeignKeySql(key, true), Is.EqualTo(sqlite + suffix));
		Assert.That(
			sqliteWriter.ForeignKeySql(key, false), Is.EqualTo(sqlite + suffix + ",\n"));
	}

	private class WriterProbe : SqlWriter
	{
		public string ColumnSql(Column column, bool isLast) =>
			GetColumnSql(column, isLast);

#pragma warning disable CS0618 // Exercise the supported legacy subclass API.
		public string LegacyColumnSql(Column column) => GetCreateColumnSql(column);
#pragma warning restore CS0618

		public string ForeignKeySql(ForeignKey key, bool isLast) =>
			GetForeignKeySql(key, isLast);
	}

	private class AccessWriterProbe : SqlWriterOleDb
	{
		public string ColumnSql(Column column, bool isLast) =>
			GetColumnSql(column, isLast);

#pragma warning disable CS0618 // Exercise the supported legacy subclass API.
		public string LegacyColumnSql(Column column) => GetCreateColumnSql(column);
#pragma warning restore CS0618

		public string ForeignKeySql(ForeignKey key, bool isLast) =>
			GetForeignKeySql(key, isLast);
	}

	private sealed class SqliteWriterProbe : SqlWriterSqlite
	{
		public string ForeignKeySql(ForeignKey key, bool isLast) =>
			GetForeignKeySql(key, isLast);
	}

	private sealed class CustomizedWriter : WriterProbe
	{
		protected override string GetColumnSql(Column column, bool isLast) =>
			throw new InvalidOperationException("Unexpected column override call.");

		protected override string GetColumnTypeText(Column column) => " CUSTOM";

		protected override string GetIdentityKeywordText(Column column) =>
			" GENERATED";
	}

	private sealed class CustomizedAccessWriter : AccessWriterProbe
	{
		protected override string GetColumnSql(Column column, bool isLast) =>
			throw new InvalidOperationException("Unexpected column override call.");

		protected override string GetColumnTypeText(Column column) => " CUSTOM";
	}
}
