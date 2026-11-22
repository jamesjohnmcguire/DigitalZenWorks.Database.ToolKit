//////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterTextLiteralTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
//////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using Microsoft.Data.Sqlite;
using NUnit.Framework;

/// <summary>
/// Verifies raw-text formatting separately from vocabulary cleanup.
/// </summary>
[TestFixture]
internal sealed class SqlWriterTextLiteralTests
{
	/// <summary>
	/// Standard SQL quoting preserves raw data and distinguishes nulls.
	/// </summary>
	/// <param name="value">The raw input.</param>
	/// <param name="expected">The complete SQL literal.</param>
	[TestCase(null, "NULL")]
	[TestCase("", "''")]
	[TestCase("NULL", "'NULL'")]
	[TestCase(" \t ", "' \t '")]
	[TestCase("  日本語  ", "'  日本語  '")]
	[TestCase("か\u3099", "'か\u3099'")]
	[TestCase("a\r\nb\nc\td", "'a\r\nb\nc\td'")]
	[TestCase("O'Brien", "'O''Brien'")]
	[TestCase("''", "''''''")]
	[TestCase("'quoted'", "'''quoted'''")]
	[TestCase(@"a\b", @"'a\b'")]
	[TestCase("x'); DROP TABLE Records; --", "'x''); DROP TABLE Records; --'")]
	public void StandardTextLiteralPreservesRawData(
		string? value, string expected)
	{
		SqlWriter writer = new();
		string result = writer.GetTextLiteral(value);

		Assert.That(result, Is.EqualTo(expected));
	}

	/// <summary>
	/// MySQL formatting escapes raw text through a base-class reference.
	/// </summary>
	/// <param name="value">The raw input.</param>
	/// <param name="expected">The complete MySQL literal.</param>
	[TestCase(null, "NULL")]
	[TestCase("", "''")]
	[TestCase("NULL", "'NULL'")]
	[TestCase(" \t ", "' \t '")]
	[TestCase("  日本語  ", "'  日本語  '")]
	[TestCase("か\u3099", "'か\u3099'")]
	[TestCase("a\r\nb\nc\td", "'a\r\nb\nc\td'")]
	[TestCase("O'Brien", @"'O\'Brien'")]
	[TestCase("''", @"'\'\''")]
	[TestCase("'quoted'", @"'\'quoted\''")]
	[TestCase(@"a\b", @"'a\\b'")]
	[TestCase("\"quoted\"", "'\\\"quoted\\\"'")]
	[TestCase("x'); DROP TABLE Records; --", @"'x\'); DROP TABLE Records; --'")]
	public void MySqlTextLiteralPreservesRawData(
		string? value, string expected)
	{
		SqlWriter writer = new SqlWriterMySql();
		Assert.That(writer.GetTextLiteral(value), Is.EqualTo(expected));
	}

	/// <summary>
	/// Executing standard literals recovers the original text or SQL null.
	/// </summary>
	/// <param name="value">The value to round-trip through SQLite.</param>
	[TestCase(null)]
	[TestCase("")]
	[TestCase("NULL")]
	[TestCase("  日本語\tか\u3099\r\n  ")]
	[TestCase("''")]
	[TestCase(@"a\b")]
	[TestCase("x'); DROP TABLE Records; --")]
	public void StandardTextLiteralRoundTrips(string? value)
	{
		using SqliteConnection connection = new("Data Source=:memory:");
		connection.Open();
		using SqliteCommand command = connection.CreateCommand();
		SqlWriter writer = new();
#pragma warning disable CA2100 // Execute the generated literal under test.
		command.CommandText = "SELECT " + writer.GetTextLiteral(value);
#pragma warning restore CA2100

		object? result = command.ExecuteScalar();

		Assert.That(result, Is.EqualTo((object?)value ?? DBNull.Value));
	}

	/// <summary>
	/// The obsolete helpers retain their published quoting behavior.
	/// </summary>
	[Test]
	public void LegacyEscapingPreservesPublishedBehavior()
	{
#pragma warning disable CS0618 // Verify the retained escaping APIs.
		string? quoted = SqlWriter.EscapeString("  O'Brien  ");
		string? escaped = SqlWriter.GetEscapedValue("a''b");
		string? escapedColumn = SqlWriter.GetEscapedValue(["a''b'c"], 0);
#pragma warning restore CS0618

		Assert.That(quoted, Is.EqualTo("'O'Brien'"));
		Assert.That(escaped, Is.EqualTo("a''b"));
		Assert.That(escapedColumn, Is.EqualTo("a''''b''c"));
	}

	/// <summary>
	/// The row helper still applies its legacy quoting and null rules.
	/// </summary>
	[Test]
	public void LegacyRowValuesPreservePublishedBehavior()
	{
		string[] values = ["  O'Brien  ", " "];
#pragma warning disable CS0618 // Verify the retained static row API.
		string? escaped = SqlWriter.GetSqlValue(values, 0);
		string? missing = SqlWriter.GetSqlValue(values, 1);
#pragma warning restore CS0618

		Assert.That(escaped, Is.EqualTo("O''Brien"));
		Assert.That(missing, Is.EqualTo("'NULL'"));
	}

	/// <summary>
	/// The legacy null helpers preserve their distinct missing-value rules.
	/// </summary>
	/// <param name="value">The input value.</param>
	/// <param name="expectedValue">The legacy SQL value.</param>
	/// <param name="expectedNull">Whether the legacy predicate accepts it.</param>
	[TestCase(null, "NULL", true)]
	[TestCase("", "NULL", false)]
	[TestCase(" \t ", "NULL", false)]
	[TestCase("NULL", "NULL", true)]
	[TestCase("null", "null", false)]
	[TestCase(" NULL ", " NULL ", false)]
	public void LegacyNullHelpersPreservePublishedBehavior(
		string? value, string expectedValue, bool expectedNull)
	{
#pragma warning disable CS0618 // Verify the retained null APIs.
		string sqlValue = SqlWriter.GetSqlNullValue(value);
		bool isNull = SqlWriter.IsNullOrSqlNull(value!);
#pragma warning restore CS0618

		Assert.That(sqlValue, Is.EqualTo(expectedValue));
		Assert.That(isNull, Is.EqualTo(expectedNull));
	}

	/// <summary>
	/// The instance row helper still supports the legacy default settings.
	/// </summary>
	[Test]
	public void LegacyInstanceRowHelperPreservesDefaults()
	{
		SqlWriter writer = new([" \t ", "  a\t  b  "]);
#pragma warning disable CS0618 // Verify the retained instance row API.
		string? fallback = writer.GetSqlValue(0, false, false, "fallback");
		string? sqlNull = writer.GetSqlValue(0, false, true, "fallback");
		string? cleaned = writer.GetSqlValue(1);
#pragma warning restore CS0618

		Assert.That(fallback, Is.EqualTo("fallback"));
		Assert.That(sqlNull, Is.EqualTo("NULL"));
		Assert.That(cleaned, Is.EqualTo("a b"));
	}
}
