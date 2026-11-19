/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterTextLiteralTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

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
}
