/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataDefinitionOleDbTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Versioning;
using NUnit.Framework;

/// <summary>
/// DataDefinitionOleDbTests class.
/// </summary>
internal sealed class DataDefinitionOleDbTests
{
	/// <summary>
	/// Export retains the facade's exception policy for schema reader failures.
	/// </summary>
	/// <param name="exceptionType">The exception from the schema reader.</param>
	/// <param name="handled">Whether export returns false for the failure.</param>
	[TestCase(typeof(ArgumentNullException), true)]
	[TestCase(typeof(ArgumentException), true)]
	[TestCase(typeof(InvalidOperationException), true)]
	[TestCase(typeof(IOException), false)]
	[NonParallelizable]
	[SupportedOSPlatform("windows")]
	public void ExportPreservesExceptionPolicy(Type exceptionType, bool handled)
	{
		Func<string, Collection<Table>> original =
			DataDefinitionOleDb.SchemaLoader;
		Exception failure = (Exception)Activator.CreateInstance(exceptionType);
		try
		{
			DataDefinitionOleDb.SchemaLoader = path => throw failure;
			if (handled)
			{
				Assert.That(
					DataDefinitionOleDb.ExportSchema("unused.accdb", null),
					Is.False);
			}
			else
			{
				Exception actual = Assert.Catch(
					() => DataDefinitionOleDb.ExportSchema("unused.accdb", null));
				Assert.That(actual, Is.SameAs(failure));
			}
		}
		finally
		{
			DataDefinitionOleDb.SchemaLoader = original;
		}
	}

	/// <summary>
	/// Empty schemas overwrite an existing file; invalid destinations fail.
	/// </summary>
	/// <param name="toSqlite">Whether to use the SQLite dialect.</param>
	[TestCase(false)]
	[TestCase(true)]
	[NonParallelizable]
	[SupportedOSPlatform("windows")]
	public void EmptySchemaRetainsFileOutput(bool toSqlite)
	{
		Func<string, Collection<Table>> original =
			DataDefinitionOleDb.SchemaLoader;
		string file = Path.GetTempFileName();
		try
		{
			DataDefinitionOleDb.SchemaLoader = path => [];
			File.WriteAllText(file, "previous schema");
			Assert.That(
				DataDefinitionOleDb.ExportSchema("unused.accdb", file, toSqlite),
				Is.True);
			Assert.That(File.ReadAllText(file), Is.Empty);
			Assert.That(
				DataDefinitionOleDb.ExportSchema("unused.accdb", null, toSqlite),
				Is.False);
		}
		finally
		{
			DataDefinitionOleDb.SchemaLoader = original;
			File.Delete(file);
		}
	}

	/// <summary>
	/// What: DataDefinitionOleDb.GetWriter returns the correct SqlWriter.
	/// </summary>
	[Test]
	public void GetWriterToSqliteFalseReturnsSqlWriterSqlite()
	{
		SqlWriter writer = DataDefinitionOleDb.GetWriter(false);
		Assert.That(writer, Is.InstanceOf<SqlWriterOleDb>());
	}

	/// <summary>
	/// What: DataDefinitionOleDb.GetWriter returns the correct SqlWriter.
	/// </summary>
	[Test]
	public void GetWriterToSqliteTrueReturnsSqlWriterSqlite()
	{
		SqlWriter writer = DataDefinitionOleDb.GetWriter(true);
		Assert.That(writer, Is.InstanceOf<SqlWriterSqlite>());
	}
}
