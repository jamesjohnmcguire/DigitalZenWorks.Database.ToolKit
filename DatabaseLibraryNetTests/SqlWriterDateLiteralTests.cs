/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterDateLiteralTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using NUnit.Framework;

/// <summary>
/// Verifies date formatting and compatibility with the row-based API.
/// </summary>
[TestFixture]
internal sealed class SqlWriterDateLiteralTests
{
	/// <summary>
	/// The row adapter retains its OLE Automation date behavior.
	/// </summary>
	/// <param name="field">The original field.</param>
	/// <param name="expected">The SQL date literal.</param>
	[TestCase("  2.5\t", "'1900-01-01'")]
	[TestCase("0", "'1899-12-30'")]
	[TestCase(null, "NULL")]
	[TestCase("", "NULL")]
	[TestCase(" \t ", "NULL")]
	[TestCase("invalid", "NULL")]
	[TestCase("NULL", "NULL")]
	[TestCase("'2.5'", "NULL")]
	[SetCulture("en-US")]
	public void GetDatePreservesRowBehavior(string? field, string expected)
	{
		SqlWriter writer = new([field!]);
		string? result = writer.GetDate(0);

		Assert.That(result, Is.EqualTo(expected));
	}

	/// <summary>
	/// OLE Automation input is still parsed in the current culture.
	/// </summary>
	[Test]
	[SetCulture("fr-FR")]
	public void GetDateParsesTheCurrentCulture()
	{
		SqlWriter writer = new(["2,5"]);
		string? result = writer.GetDate(0);

		Assert.That(result, Is.EqualTo("'1900-01-01'"));
	}

	/// <summary>
	/// The row adapter reports a number outside the supported date range.
	/// </summary>
	[Test]
	public void GetDateRejectsAnOutOfRangeSerial()
	{
		SqlWriter writer = new(["3000000"]);

		Assert.That(() => writer.GetDate(0), Throws.TypeOf<ArgumentException>());
	}

	/// <summary>
	/// A typed date is formatted without input parsing or culture rules.
	/// </summary>
	[Test]
	[SetCulture("th-TH")]
	public void DateLiteralUsesAnInvariantCalendar()
	{
		SqlWriter writer = new();
		DateTime date = new(2024, 2, 29, 15, 30, 0);
		string result = writer.GetDateLiteral(date);

		Assert.That(result, Is.EqualTo("'2024-02-29'"));
	}

	/// <summary>
	/// A missing typed date produces the SQL null keyword.
	/// </summary>
	[Test]
	public void DateLiteralFormatsNull()
	{
		SqlWriter writer = new();
		string result = writer.GetDateLiteral(null);

		Assert.That(result, Is.EqualTo("NULL"));
	}
}
