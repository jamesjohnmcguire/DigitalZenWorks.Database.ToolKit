//////////////////////////////////////////////////////////////////////////////
// <copyright file="ColumnPairTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 James John McGuire.
// </copyright>
//////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using NUnit.Framework;

/// <summary>
/// Verifies the contract for a column identifier and its SQL value.
/// </summary>
[TestFixture]
internal sealed class ColumnPairTests
{
	/// <summary>
	/// Valid SQL values and identifiers are kept as supplied.
	/// </summary>
	/// <param name="sqlValue">The complete SQL value to store.</param>
	[TestCase("NULL")]
	[TestCase("''")]
	[TestCase("  '日本語'  ")]
	public void PreservesPreparedSqlText(string sqlValue)
	{
		const string ColumnName = "\"word\"";
		ColumnPair pair = new(ColumnName, sqlValue);

		Assert.Multiple(() =>
		{
			Assert.That(pair.ColumnName, Is.EqualTo(ColumnName));
			Assert.That(pair.SqlValue, Is.EqualTo(sqlValue));
		});
	}

	/// <summary>
	/// Rejects missing or blank identifiers with the parameter name.
	/// </summary>
	/// <param name="columnName">The invalid column identifier.</param>
	/// <param name="exceptionType">The expected exception type.</param>
	[TestCase(null, typeof(ArgumentNullException))]
	[TestCase("", typeof(ArgumentException))]
	[TestCase(" \t ", typeof(ArgumentException))]
	public void RejectsInvalidColumnNames(
		string? columnName, Type exceptionType)
	{
		Assert.That(
			() => new ColumnPair(columnName!, "NULL"),
			Throws.TypeOf(exceptionType)
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo(nameof(columnName)));
	}

	/// <summary>
	/// Rejects missing or blank SQL values with the parameter name.
	/// </summary>
	/// <param name="sqlValue">The invalid SQL value.</param>
	/// <param name="exceptionType">The expected exception type.</param>
	[TestCase(null, typeof(ArgumentNullException))]
	[TestCase("", typeof(ArgumentException))]
	[TestCase(" \t ", typeof(ArgumentException))]
	public void RejectsInvalidSqlValues(string? sqlValue, Type exceptionType)
	{
		Assert.That(
			() => new ColumnPair("word", sqlValue!),
			Throws.TypeOf(exceptionType)
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo(nameof(sqlValue)));
	}
}
