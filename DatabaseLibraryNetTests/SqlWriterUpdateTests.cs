//////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterUpdateTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 James John McGuire.
// </copyright>
//////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// Covers column-pair updates and compatibility with the legacy overload.
/// </summary>
[TestFixture]
internal sealed class SqlWriterUpdateTests
{
	/// <summary>
	/// Both overloads preserve assignment and composite key order.
	/// </summary>
	[Test]
	public void UpdatePreservesPreparedAssignmentsAndCompositeKeys()
	{
		SqlWriter writer = new SqlWriterMySql();

		string statement = writer.CreateUpdateStatement(
			"`Words`",
			[new("`language`", "'jp'"), new("`id`", "42")],
			[new("`word`", "'日本語'"), new("`notes`", "NULL")]);
		string? legacy = SqlWriter.CreateUpdateStatement(
			"`Words`",
			[new("`language`", "'jp'"), new("`id`", "42")],
			[new("`word`", "'日本語'"), new("`notes`", "NULL")]);

		const string Expected = "UPDATE `Words` SET `word` = '日本語', " +
			"`notes` = NULL WHERE `language` = 'jp' AND `id` = 42;";
		Assert.That(statement, Is.EqualTo(Expected));
		Assert.That(legacy, Is.EqualTo(Expected));
	}

	/// <summary>
	/// A single assignment and key need no separators.
	/// </summary>
	[Test]
	public void UpdateAcceptsOneAssignmentAndOneKey()
	{
		SqlWriter writer = new();

		string statement = writer.CreateUpdateStatement(
			"Words", [new("id", "42")], [new("count", "count + 1")]);
		const string Expected = "UPDATE Words SET count = count + 1 " +
			"WHERE id = 42;";

		Assert.That(statement, Is.EqualTo(Expected));
	}

	/// <summary>
	/// Invalid table names are rejected with the original parameter name.
	/// </summary>
	/// <param name="tableName">The invalid table identifier.</param>
	/// <param name="exceptionType">The expected exception type.</param>
	[TestCase(null, typeof(ArgumentNullException))]
	[TestCase("", typeof(ArgumentException))]
	[TestCase(" ", typeof(ArgumentException))]
	public void UpdateRejectsInvalidTableNames(
		string? tableName, Type exceptionType)
	{
		SqlWriter writer = new();

		Assert.That(
			() => writer.CreateUpdateStatement(
				tableName!, [new("id", "1")], [new("word", "''")]),
			Throws.TypeOf(exceptionType)
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo(nameof(tableName)));
	}

	/// <summary>
	/// Both assignments and key conditions require valid column pairs.
	/// </summary>
	/// <param name="validateKeys">Whether to pass invalid key pairs.</param>
	[TestCase(true)]
	[TestCase(false)]
	public void UpdateRejectsMissingColumnPairs(bool validateKeys)
	{
		SqlWriter writer = new();
		IReadOnlyList<ColumnPair> keys = [new("id", "1")];
		IReadOnlyList<ColumnPair> assignments = [new("word", "''")];
		string parameterName =
			validateKeys ? "primaryKeyPairs" : "columnPairs";

		string CreateStatement(IReadOnlyList<ColumnPair> pairs)
		{
			string statement = writer.CreateUpdateStatement(
				"Words",
				validateKeys ? pairs : keys,
				validateKeys ? assignments : pairs);

			return statement;
		}

		Assert.That(
			() => CreateStatement(null!),
			Throws.ArgumentNullException
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo(parameterName));
		Assert.That(
			() => CreateStatement([]),
			Throws.ArgumentException
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo(parameterName));
		Assert.That(
			() => CreateStatement([null!]),
			Throws.ArgumentException
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo(parameterName));
	}
}
