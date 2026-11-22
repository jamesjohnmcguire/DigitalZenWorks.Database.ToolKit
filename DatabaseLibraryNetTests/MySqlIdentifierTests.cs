//////////////////////////////////////////////////////////////////////////////
// <copyright file="MySqlIdentifierTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 James John McGuire.
// </copyright>
//////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using NUnit.Framework;

/// <summary>
/// Verifies the MySQL quoting rules for individual identifiers.
/// </summary>
[TestFixture]
internal sealed class MySqlIdentifierTests
{
	/// <summary>
	/// Raw names are quoted and already quoted names remain unchanged.
	/// </summary>
	/// <param name="identifier">The name to quote.</param>
	/// <param name="expected">The expected quoted form.</param>
	[TestCase("word", "`word`")]
	[TestCase("select", "`select`")]
	[TestCase("word name", "`word name`")]
	[TestCase("日本語", "`日本語`")]
	[TestCase("word`name", "`word``name`")]
	[TestCase("word.name", "`word.name`")]
	[TestCase("`word`", "`word`")]
	[TestCase("`word``name`", "`word``name`")]
	[TestCase("```word```", "```word```")]
	public void EnsuresOneQuotedIdentifier(string identifier, string expected)
	{
		string result = SqlWriterMySql.EnsureQuotedIdentifier(identifier);

		Assert.That(result, Is.EqualTo(expected));
	}

	/// <summary>
	/// Invalid names are rejected with their parameter name.
	/// </summary>
	/// <param name="identifier">The invalid identifier.</param>
	/// <param name="exceptionType">The expected exception type.</param>
	[TestCase(null, typeof(ArgumentNullException))]
	[TestCase("", typeof(ArgumentException))]
	[TestCase("  ", typeof(ArgumentException))]
	[TestCase("a\0b", typeof(ArgumentException))]
	[TestCase("`", typeof(ArgumentException))]
	[TestCase("``", typeof(ArgumentException))]
	[TestCase("`word", typeof(ArgumentException))]
	[TestCase("word`", typeof(ArgumentException))]
	[TestCase("`word`name`", typeof(ArgumentException))]
	[TestCase("`word``", typeof(ArgumentException))]
	[TestCase("`db`.`word`", typeof(ArgumentException))]
	public void RejectsInvalidIdentifiers(
		string? identifier, Type exceptionType)
	{
		Assert.That(
			() => SqlWriterMySql.EnsureQuotedIdentifier(identifier!),
			Throws.TypeOf(exceptionType)
				.With.Property(nameof(ArgumentException.ParamName))
				.EqualTo(nameof(identifier)));
	}
}
