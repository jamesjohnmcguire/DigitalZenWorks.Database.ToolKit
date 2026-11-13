// <copyright file="ArgumentGuardTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using NUnit.Framework;

/// <summary>
/// Verifies the shared null guard's argument and exception contracts.
/// </summary>
[TestFixture]
internal sealed class ArgumentGuardTests
{
	/// <summary>
	/// Null arguments retain the supplied name and framework default message.
	/// </summary>
	/// <param name="parameterName">The name forwarded by the caller.</param>
	[TestCase("table")]
	[TestCase("column")]
	[TestCase("database")]
	[TestCase(null)]
	[TestCase("")]
	public void NullArgumentPreservesExceptionDetails(string parameterName)
	{
		ArgumentNullException expected = new(parameterName);

		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
			() => ArgumentGuard.ThrowIfNull(null, parameterName));

		Assert.That(exception.ParamName, Is.EqualTo(parameterName));
		Assert.That(exception.Message, Is.EqualTo(expected.Message));
	}

	/// <summary>
	/// Empty strings, whitespace, and boxed default values are non-null.
	/// </summary>
	/// <param name="argument">The non-null argument to validate.</param>
	[TestCase("")]
	[TestCase(" ")]
	[TestCase("value")]
	[TestCase(0)]
	[TestCase(false)]
	public void NonNullValuesAreAccepted(object argument)
	{
		Assert.DoesNotThrow(() =>
			ArgumentGuard.ThrowIfNull(argument, nameof(argument)));
	}

	/// <summary>
	/// Reference objects and empty arrays also pass without exceptions.
	/// </summary>
	[Test]
	public void NonNullObjectsAndArraysAreAccepted()
	{
		object argument = new();
		object[] items = [];

		Assert.DoesNotThrow(() =>
			ArgumentGuard.ThrowIfNull(argument, nameof(argument)));
		Assert.DoesNotThrow(() =>
			ArgumentGuard.ThrowIfNull(items, nameof(items)));
	}
}
