/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConsolidationTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Runtime.Versioning;
using NUnit.Framework;

/// <summary>
/// Characterizes the contracts shared by overlapping entry points.
/// </summary>
[TestFixture]
internal sealed class ConsolidationTests
{
	/// <summary>
	/// Declaration parsing and schema metadata retain their distinct rules.
	/// </summary>
	/// <param name="text">The declaration or metadata text.</param>
	/// <param name="declaration">The expected declaration type.</param>
	/// <param name="metadata">The expected metadata type.</param>
	[TestCase(null, ColumnType.Other, ColumnType.Unknown)]
	[TestCase("", ColumnType.Other, ColumnType.Unknown)]
	[TestCase("unknown", ColumnType.Other, ColumnType.Unknown)]
	[TestCase("BIGINT", ColumnType.BigInt, ColumnType.Number)]
	[TestCase("smallint", ColumnType.SmallInt, ColumnType.Number)]
	[TestCase("integer", ColumnType.Integer, ColumnType.Number)]
	[TestCase("int", ColumnType.Int, ColumnType.Number)]
	[TestCase(" numeric (12,2) ", ColumnType.Numeric, ColumnType.Numeric)]
	[TestCase("decimal(9,3)", ColumnType.Decimal, ColumnType.Decimal)]
	[TestCase("dec", ColumnType.Other, ColumnType.Decimal)]
	[TestCase("real", ColumnType.Real, ColumnType.Real)]
	[TestCase("float", ColumnType.Float, ColumnType.Float)]
	[TestCase("double precision", ColumnType.Double, ColumnType.Double)]
	[TestCase("currency", ColumnType.Currency, ColumnType.Currency)]
	[TestCase("money", ColumnType.Money, ColumnType.Money)]
	[TestCase("smallmoney", ColumnType.SmallMoney, ColumnType.SmallMoney)]
	[TestCase("varchar(42)", ColumnType.VarChar, ColumnType.String)]
	[TestCase("nvarchar(42)", ColumnType.VarChar, ColumnType.String)]
	[TestCase("nchar(42)", ColumnType.Char, ColumnType.String)]
	[TestCase("character varying", ColumnType.Char, ColumnType.String)]
	[TestCase("string", ColumnType.String, ColumnType.String)]
	[TestCase("text", ColumnType.Text, ColumnType.Text)]
	[TestCase("blob", ColumnType.Blob, ColumnType.Blob)]
	[TestCase("date", ColumnType.Date, ColumnType.DateTime)]
	[TestCase("timestamp", ColumnType.Timestamp, ColumnType.DateTime)]
	[TestCase("datetime", ColumnType.DateTime, ColumnType.DateTime)]
	[TestCase("datetime2", ColumnType.DateTime2, ColumnType.Unknown)]
	[TestCase("YESNO", ColumnType.Boolean, ColumnType.Unknown)]
	[TestCase("price DECIMAL(9,2)", ColumnType.Decimal, ColumnType.Unknown)]
	[TestCase("point", ColumnType.Int, ColumnType.Unknown)]
	public void ColumnTypeEntryPointsPreserveTheirInterpretations(
		string text, ColumnType declaration, ColumnType metadata)
	{
		using SchemaProbe schema = new();
		Assert.That(DataDefinition.GetColumnType(text), Is.EqualTo(declaration));
		Assert.That(schema.ReadType(text), Is.EqualTo(metadata));
	}

	/// <summary>
	/// Both ordering entry points preserve cycles, missing nodes and order.
	/// </summary>
	[Test]
	public void DependencyOrderingPreservesTraversalAndInput()
	{
		Dictionary<string, Collection<string>> dependencies = new()
		{
			["Orders"] = ["Products", "External", "Products"],
			["Products"] = ["Categories"],
			["Categories"] = ["Products"],
			["Self"] = ["Self"],
		};
		string[] expected =
			["Categories", "Products", "External", "Orders", "Self"];

		Assert.That(
			DataDefinition.GetOrderedDependencies(dependencies),
			Is.EqualTo(expected));
		Assert.That(
			DataStoreStructure.GetOrderedDependencies(dependencies),
			Is.EqualTo(expected));
		Collection<string> originalOrders = ["Products", "External", "Products"];
		Assert.That(dependencies["Orders"], Is.EqualTo(originalOrders));
		Assert.That(DataDefinition.GetOrderedDependencies(null), Is.Empty);
		Assert.That(DataStoreStructure.GetOrderedDependencies(null), Is.Empty);
	}

	/// <summary>
	/// Metadata actions remain case-sensitive and unknown actions default.
	/// </summary>
	/// <param name="action">The provider action text.</param>
	/// <param name="expected">The expected action.</param>
	[TestCase("CASCADE", ConstraintAction.Cascade)]
	[TestCase("SET NULL", ConstraintAction.SetNull)]
	[TestCase("NO ACTION", ConstraintAction.NoAction)]
	[TestCase("cascade", ConstraintAction.NoAction)]
	[TestCase(" CASCADE ", ConstraintAction.NoAction)]
	[TestCase("RESTRICT", ConstraintAction.NoAction)]
	[TestCase(null, ConstraintAction.NoAction)]
	public void SchemaRelationshipPreservesMetadata(
		string action, ConstraintAction expected)
	{
		using SchemaProbe schema = new();
		using DataTable table = CreateRelationshipMetadata(false);
		DataRow row = table.Rows.Add(
			DBNull.Value,
			"Orders",
			"productId",
			"Products",
			"id",
			action,
			"SET NULL");
		Relationship relationship = schema.ReadRelationship(row);

		AssertRelationship(relationship, expected);
		Assert.That(
			() => schema.ReadRelationship(null),
			Throws.ArgumentNullException.With.Property("ParamName")
			.EqualTo("foreignKey"));
	}

	/// <summary>
	/// OleDb retains its metadata names and permissive null-row behavior.
	/// </summary>
	/// <param name="action">The provider action text.</param>
	/// <param name="expected">The expected action.</param>
	[TestCase("CASCADE", ConstraintAction.Cascade)]
	[TestCase("SET NULL", ConstraintAction.SetNull)]
	[TestCase("cascade", ConstraintAction.NoAction)]
	[TestCase(null, ConstraintAction.NoAction)]
	[SupportedOSPlatform("windows")]
	public void OleDbRelationshipPreservesMetadata(
		string action, ConstraintAction expected)
	{
		using OleDbProbe schema = new();
		using DataTable table = CreateRelationshipMetadata(true);
		DataRow row = table.Rows.Add(
			DBNull.Value,
			"Orders",
			"productId",
			"Products",
			"id",
			action,
			"SET NULL");
		AssertRelationship(schema.ReadRelationship(row), expected);
		Assert.That(schema.ReadRelationship(null).Name, Is.Null);
	}

	/// <summary>
	/// Connection creation keeps supported providers and unopened state.
	/// </summary>
	/// <param name="databaseType">The requested provider.</param>
	/// <param name="expectedType">The expected concrete connection.</param>
	[TestCase(DatabaseType.SQLite, typeof(SQLiteConnection))]
	[TestCase(DatabaseType.MySql, typeof(MySql.Data.MySqlClient.MySqlConnection))]
	[TestCase(
		DatabaseType.SqlServer, typeof(Microsoft.Data.SqlClient.SqlConnection))]
	[TestCase(DatabaseType.OleDb, null)]
	[TestCase(DatabaseType.Oracle, null)]
	[TestCase(DatabaseType.Unknown, null)]
	public void StorageConnectionFactoryPreservesProviderSupport(
		DatabaseType databaseType, Type expectedType)
	{
		using StorageProbe storage = new();
		using DbConnection connection = storage.Create(databaseType);
		if (expectedType == null)
		{
			Assert.That(connection, Is.Null);
		}
		else
		{
			Assert.That(connection, Is.TypeOf(expectedType));
			Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));
		}
	}

	/// <summary>
	/// The Dapper facade closes connections it opens for a query.
	/// </summary>
	[Test]
	public void DatabaseConnectionRetainsQueryAndTransactionBehavior()
	{
		using DatabaseConnection database = new("Data Source=:memory:");
		Assert.That(
			database.QuerySingleOrDefault<int>("SELECT 42", null),
			Is.EqualTo(42));
		Assert.That(
			() => database.BeginTransaction(),
			Throws.InvalidOperationException);
	}

	/// <summary>
	/// Access validation keeps header precedence and extension fallback.
	/// </summary>
	[Test]
	[SupportedOSPlatform("windows")]
	public void AccessValidationPreservesFileDetection()
	{
		string directory = Path.Combine(
			Path.GetTempPath(), Guid.NewGuid().ToString());
		Directory.CreateDirectory(directory);
		string path = Path.Combine(directory, "sample.accdb");
		try
		{
			Assert.That(OleDbHelper.ValidateAccessDatabaseFile(path), Is.False);
			File.WriteAllText(path, string.Empty);
			Assert.That(OleDbHelper.ValidateAccessDatabaseFile(path), Is.True);
			File.WriteAllText(path, "SQLite format 3\0");
			Assert.That(
				() => OleDbHelper.ValidateAccessDatabaseFile(path),
				Throws.ArgumentException.With.Property("ParamName")
				.EqualTo("databaseFile"));
		}
		finally
		{
			Directory.Delete(directory, true);
		}
	}

	private static DataTable CreateRelationshipMetadata(bool oleDb)
	{
		string[] names;
		if (oleDb)
		{
			names =
			[
				"FK_NAME", "FK_TABLE_NAME", "FK_COLUMN_NAME",
				"PK_TABLE_NAME", "PK_COLUMN_NAME", "DELETE_RULE", "UPDATE_RULE"
			];
		}
		else
		{
			names =
			[
				"CONSTRAINT_NAME", "TABLE_NAME", "FKEY_FROM_COLUMN",
				"FKEY_TO_TABLE", "FKEY_TO_COLUMN",
				"FKEY_ON_DELETE", "FKEY_ON_UPDATE"
			];
		}

		DataTable table = new();
		foreach (string name in names)
		{
			table.Columns.Add(name);
		}

		return table;
	}

	private static void AssertRelationship(
		Relationship relationship, ConstraintAction expected)
	{
		Assert.That(relationship.Name, Is.Empty);
		Assert.That(relationship.ReferencingTable, Is.EqualTo("Orders"));
		Assert.That(relationship.ReferencingColumn, Is.EqualTo("productId"));
		Assert.That(relationship.ReferencedTable, Is.EqualTo("Products"));
		Assert.That(relationship.ReferencedColumn, Is.EqualTo("id"));
		Assert.That(relationship.OnDeleteAction, Is.EqualTo(expected));
		Assert.That(
			relationship.OnUpdateAction, Is.EqualTo(ConstraintAction.SetNull));
	}

	private sealed class SchemaProbe()
		: DataStoreStructure(DatabaseType.SQLite, ":memory:")
	{
		public ColumnType ReadType(string text) => GetColumnType(text, 0, 42);

		public Relationship ReadRelationship(DataRow row) => GetRelationship(row);
	}

	[SupportedOSPlatform("windows")]
	private sealed class OleDbProbe() : OleDbSchema("unused.accdb")
	{
		public Relationship ReadRelationship(DataRow row) => GetRelationship(row);
	}

	private sealed class StorageProbe : DataStorage
	{
		public DbConnection Create(DatabaseType databaseType) =>
			GetConnectionObject(databaseType, string.Empty);
	}
}
