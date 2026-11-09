// <copyright file="OleDbSchemaTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Data.OleDb;
using System.IO;
using System.Runtime.Versioning;
using NUnit.Framework;

/// <summary>
/// Tests AutoNumber detection through the public Access schema reader.
/// </summary>
[SupportedOSPlatform("windows")]
[TestFixture]
internal sealed class OleDbSchemaTests
{
	private string testDirectory;
	private string databaseFile;

	/// <summary>
	/// Creates an isolated, empty Access database for each test.
	/// </summary>
	[SetUp]
	public void Setup()
	{
		testDirectory = Path.Combine(
			Path.GetTempPath(), "OleDbSchemaTests-" + Guid.NewGuid());
		Directory.CreateDirectory(testDirectory);
		databaseFile = Path.Combine(testDirectory, "schema.accdb");

		bool created = OleDbHelper.CreateAccessDatabaseFile(databaseFile);
		Assert.That(created, Is.True);
	}

	/// <summary>
	/// Removes the temporary database after connections have been closed.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(testDirectory))
		{
			Directory.Delete(testDirectory, true);
		}
	}

	/// <summary>
	/// Distinguishes generated values from ordinary integers independently
	/// of whether the column is a primary key, even in an empty table.
	/// </summary>
	/// <param name="declaration">The Access column declaration.</param>
	/// <param name="expectedType">The expected schema column type.</param>
	/// <param name="isPrimary">Whether the column is a primary key.</param>
	[TestCase("COUNTER PRIMARY KEY", ColumnType.AutoNumber, true)]
	[TestCase("INTEGER PRIMARY KEY", ColumnType.Number, true)]
	[TestCase("COUNTER", ColumnType.AutoNumber, false)]
	[TestCase("INTEGER", ColumnType.Number, false)]
	public void GetTableDistinguishesAutoNumberFromOrdinaryInteger(
		string declaration, ColumnType expectedType, bool isPrimary)
	{
		CreateTable(declaration);

		using OleDbSchema schema = new(databaseFile);
		Table table = schema.GetTable("Detection Table");

		Assert.That(table.Columns.Count, Is.EqualTo(2));
		Assert.That(table.Columns["id"].ColumnType, Is.EqualTo(expectedType));
		Assert.That(table.Columns["id"].Primary, Is.EqualTo(isPrimary));
		Assert.That(
			table.Columns["ordinaryNumber"].ColumnType,
			Is.EqualTo(ColumnType.Number));
		Assert.That(table.Columns["ordinaryNumber"].Primary, Is.False);
	}

	/// <summary>
	/// A missing table produces a metadata exception without preventing a
	/// subsequent read through the same schema instance.
	/// </summary>
	[Test]
	public void GetTableCanReadValidTableAfterMissingTableThrows()
	{
		CreateTable("COUNTER PRIMARY KEY");

		using OleDbSchema schema = new(databaseFile);
		Exception exception = Assert.Catch(
			() => schema.GetTable("MissingTable"));
		Assert.That(
			exception,
			Is.InstanceOf<OleDbException>()
				.Or.InstanceOf<InvalidOperationException>());

		if (exception is InvalidOperationException)
		{
			Assert.That(exception.Message, Does.Contain("MissingTable"));
			Assert.That(exception.Message, Does.Contain("metadata"));
		}

		Table table = schema.GetTable("Detection Table");
		Assert.That(table.Columns.Count, Is.EqualTo(2));
		Assert.That(
			table.Columns["id"].ColumnType, Is.EqualTo(ColumnType.AutoNumber));
		Assert.That(table.Columns["id"].Primary, Is.True);
	}

	private void CreateTable(string declaration)
	{
		string connectionString =
			OleDbHelper.BuildConnectionString(databaseFile);
		using OleDbConnection connection = new(connectionString);
		connection.Open();

		using OleDbCommand command = connection.CreateCommand();
#pragma warning disable CA2100 // Declarations come only from test cases.
		command.CommandText = "CREATE TABLE [Detection Table] ([id] " +
			declaration + ", [ordinaryNumber] INTEGER)";
#pragma warning restore CA2100
		command.ExecuteNonQuery();
	}
}
