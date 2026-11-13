/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataStorageParameterTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Runtime.Versioning;
using NUnit.Framework;

/// <summary>
/// Verifies provider parameter binding and literal-value execution.
/// </summary>
[TestFixture]
internal sealed class DataStorageParameterTests
{
	/// <summary>
	/// Names, CLR values, and database nulls survive provider binding.
	/// No server connection is required to construct command parameters.
	/// </summary>
	/// <param name="databaseType">The command provider to exercise.</param>
	[TestCase(DatabaseType.SQLite)]
	[TestCase(DatabaseType.SqlServer)]
	[TestCase(DatabaseType.MySql)]
	public void ParametersPreserveNamesValuesAndTypes(DatabaseType databaseType)
	{
		using DataStorage database = new(databaseType, string.Empty);
		using DbCommand command = database.Command;
		byte[] bytes = [0, 1, 255];
		Dictionary<string, object> values = new()
		{
			{ "@text", "O'Brien'; DROP TABLE Records; --" },
			{ "@number", 42 },
			{ "@amount", 12.5m },
			{ "@binary", bytes },
			{ "@flag", true },
			{ "@missing", null },
			{ "@databaseNull", DBNull.Value },
		};

		DbParameterCollection parameters =
			DataStorage.AddParameters(command, values);

		Assert.That(parameters, Is.SameAs(command.Parameters));
		Assert.That(parameters.Count, Is.EqualTo(values.Count));
		foreach (KeyValuePair<string, object> value in values)
		{
			DbParameter parameter = parameters[value.Key];
			Assert.That(parameter.ParameterName, Is.EqualTo(value.Key));
			Assert.That(parameter.Value, Is.EqualTo(value.Value ?? DBNull.Value));
			if (value.Value != null)
			{
				Assert.That(
					parameter.Value.GetType(), Is.EqualTo(value.Value.GetType()));
			}
		}

		Assert.That(parameters["@number"].DbType, Is.EqualTo(DbType.Int32));
		Assert.That(parameters["@amount"].DbType, Is.EqualTo(DbType.Decimal));
	}

	/// <summary>
	/// SQLite binds by name and retains numeric types and SQL nulls.
	/// </summary>
	[Test]
	public void SqliteQueryBindsNamesIndependentlyOfPlaceholderOrder()
	{
		using DataStorage database = new(
			DatabaseType.SQLite, "Data Source=:memory:");
		Dictionary<string, object> values = new()
		{
			{ "@first", "first" },
			{ "@second", "second" },
			{ "@number", 42 },
			{ "@missing", null },
		};

		DataRow row = database.GetDataRow(
			"SELECT @second AS secondValue, @first AS firstValue, " +
			"typeof(@number) AS numberType, @number AS numberValue, " +
			"@missing AS missingValue",
			values);

		Assert.That(row, Is.Not.Null);
		Assert.That(row["firstValue"], Is.EqualTo("first"));
		Assert.That(row["secondValue"], Is.EqualTo("second"));
		Assert.That(row["numberType"], Is.EqualTo("integer"));
		Assert.That(row["numberValue"], Is.EqualTo(42));
		Assert.That(row["missingValue"], Is.EqualTo(DBNull.Value));
	}

	/// <summary>
	/// Quotes and SQL-looking text remain data in inserts and predicates.
	/// </summary>
	/// <param name="text">The untrusted value to round-trip.</param>
	[TestCase("O'Brien")]
	[TestCase("' OR 1=1 --")]
	[TestCase("x'); DROP TABLE ParameterChecks; --")]
	public void SqliteTreatsParameterTextAsLiteralData(string text)
	{
		using DataStorage database = new(
			DatabaseType.SQLite, "Data Source=:memory:");
		database.BeginTransaction();
		Assert.That(
			database.ExecuteNonQuery(
				"CREATE TABLE ParameterChecks (payload TEXT, marker INTEGER)"),
			Is.True);
		Dictionary<string, object> values = new()
		{
			{ "@payload", text },
			{ "@marker", 42 },
		};
		Assert.That(
			database.ExecuteNonQuery(
				"INSERT INTO ParameterChecks VALUES (@payload, @marker)",
				values),
			Is.True);
		Assert.That(
			database.ExecuteNonQuery(
				"INSERT INTO ParameterChecks VALUES ('other', 0)"),
			Is.True);

		using DataTable rows = database.GetDataTable(
			"SELECT payload, marker FROM ParameterChecks " +
			"WHERE payload = @payload AND marker = @marker",
			values);
		Assert.That(rows.Rows.Count, Is.EqualTo(1));
		Assert.That(rows.Rows[0]["payload"], Is.EqualTo(text));
		Assert.That(rows.Rows[0]["marker"], Is.EqualTo(42));
	}

	/// <summary>
	/// Access binds question-mark placeholders in supplied iteration order,
	/// not parameter-name order; names are deliberately unrelated to columns.
	/// </summary>
	[SupportedOSPlatform("windows")]
	[Test]
	public void AccessBindsMultipleParametersByPosition()
	{
		string directory = Path.Combine(
			Path.GetTempPath(), "ParameterTests-" + Guid.NewGuid());
		Directory.CreateDirectory(directory);
		try
		{
			string file = Path.Combine(directory, "parameters.accdb");
			Assert.That(OleDbHelper.CreateAccessDatabaseFile(file), Is.True);
			using DataStorageOleDb database = new(
				OleDbHelper.BuildConnectionString(file));
			Assert.That(
				database.ExecuteNonQuery(
					"CREATE TABLE ParameterChecks " +
					"(firstValue TEXT(100), secondValue TEXT(100))"),
				Is.True);
			OrderedDictionary<string, object> values = new()
			{
				{ "zFirst", "O'Brien'; DROP TABLE ParameterChecks; --" },
				{ "aSecond", "' OR 1=1 --" },
			};
			Assert.That(
				database.ExecuteNonQuery(
					"INSERT INTO ParameterChecks VALUES (?, ?)",
					values),
				Is.True);

			DataRow row = database.GetDataRow(
				"SELECT firstValue, secondValue FROM ParameterChecks " +
				"WHERE firstValue = ? AND secondValue = ?",
				values);
			Assert.That(row, Is.Not.Null);
			Assert.That(row["firstValue"], Is.EqualTo(values["zFirst"]));
			Assert.That(row["secondValue"], Is.EqualTo(values["aSecond"]));

			using OleDbCommand command = new();
			OrderedDictionary<string, object> nullValues = new()
			{
				{ "missing", null },
				{ "number", 7 },
			};
			DataStorageOleDb.AddParameters(command, nullValues);
			Assert.That(command.Parameters[0].Value, Is.EqualTo(DBNull.Value));
			Assert.That(command.Parameters[1].Value, Is.EqualTo(7));
		}
		finally
		{
			Directory.Delete(directory, true);
		}
	}
}
