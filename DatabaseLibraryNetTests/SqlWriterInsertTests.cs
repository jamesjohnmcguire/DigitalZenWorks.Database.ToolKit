//////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterInsertTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
//////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using NUnit.Framework;

/// <summary>
/// Covers the column-pair insert API and its legacy counterpart.
/// </summary>
[TestFixture]
internal sealed class SqlWriterInsertTests
{
	/// <summary>
	/// Both overloads preserve the order and supplied SQL text.
	/// </summary>
	[Test]
	public void InsertPreservesPreparedIdentifiersAndValues()
	{
		SqlWriter writer = new SqlWriterMySql();

		string statement = writer.CreateInsertStatement(
			"`Words`", [new("`id`", "42"), new("`word`", "NULL")]);
		string? legacy = SqlWriter.CreateInsertStatement(
			"`Words`", [new("`id`", "42"), new("`word`", "NULL")]);

		const string Expected =
			"INSERT INTO `Words` (`id`, `word`) VALUES (42, NULL);";
		Assert.That(statement, Is.EqualTo(Expected));
		Assert.That(legacy, Is.EqualTo(Expected));
	}

	/// <summary>
	/// The base implementation uses an unquoted column as supplied.
	/// </summary>
	[Test]
	public void InsertAcceptsASingleColumn()
	{
		SqlWriter writer = new();
		string statement =
			writer.CreateInsertStatement("Words", [new("word", "''")]);

		Assert.That(
			statement, Is.EqualTo("INSERT INTO Words (word) VALUES ('');"));
	}

	/// <summary>
	/// Invalid table names are reported before building the statement.
	/// </summary>
	/// <param name="tableName">The invalid table identifier.</param>
	/// <param name="exceptionType">The expected exception type.</param>
	[TestCase(null, typeof(ArgumentNullException))]
	[TestCase("", typeof(ArgumentException))]
	[TestCase(" ", typeof(ArgumentException))]
	public void InsertRejectsInvalidTableNames(
		string? tableName, Type exceptionType)
	{
		SqlWriter writer = new();

		Assert.That(
			() => writer.CreateInsertStatement(tableName!, [new("id", "1")]),
			Throws.TypeOf(exceptionType)
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo(nameof(tableName)));
	}

	/// <summary>
	/// A new insert requires a nonempty collection of column pairs.
	/// </summary>
	[Test]
	public void InsertRejectsMissingColumnPairs()
	{
		SqlWriter writer = new();

		Assert.That(
			() => writer.CreateInsertStatement("Words", null!),
			Throws.ArgumentNullException
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo("columnPairs"));
		Assert.That(
			() => writer.CreateInsertStatement("Words", []),
			Throws.ArgumentException
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo("columnPairs"));
		Assert.That(
			() => writer.CreateInsertStatement(
				"Words", [new("id", "1"), null!]),
			Throws.ArgumentException
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo("columnPairs"));
	}
}
