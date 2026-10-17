/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataDefinitionOleDbTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit.Tests;

using NUnit.Framework;

/// <summary>
/// DataDefinitionOleDbTests class.
/// </summary>
internal sealed class DataDefinitionOleDbTests
{
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
