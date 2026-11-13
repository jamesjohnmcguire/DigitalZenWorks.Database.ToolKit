// <copyright file="LegacyApiCompatibilityTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using NUnit.Framework;

/// <summary>
/// Protects restored published signatures and their historical behavior.
/// </summary>
[TestFixture]
internal sealed class LegacyApiCompatibilityTests
{
	/// <summary>
	/// Legacy relationship operations remain available with migration advice.
	/// </summary>
	/// <param name="declaringType">The class declaring the legacy member.</param>
	/// <param name="methodName">The legacy method name.</param>
	/// <param name="replacement">The accessible replacement guidance.</param>
	[TestCase(
		typeof(DataStoreStructure),
		"GetForeignKeyRelationships",
		"ForeignKey.Create")]
	[TestCase(
		typeof(DataStoreStructure),
		"SetForeignKeys",
		"Table.ForeignKeys")]
	[TestCase(
		typeof(DataStoreStructure),
		"GetForeignKeyRelationship",
		"ForeignKey.Create")]
	[TestCase(
		typeof(DataStoreStructure),
		"GetRelationship",
		"GetForeignKey")]
	[TestCase(
		typeof(OleDbSchema),
		"GetRelationship",
		"GetForeignKey")]
	public void RelationshipMethodsRemainAvailableWithMigrationGuidance(
		Type declaringType, string methodName, string replacement)
	{
		BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
			BindingFlags.Static | BindingFlags.Instance |
			BindingFlags.DeclaredOnly;
		MethodInfo method = declaringType.GetMethod(methodName, flags);
		Assert.That(method, Is.Not.Null);
		ObsoleteAttribute obsolete =
			method.GetCustomAttribute<ObsoleteAttribute>();

		Assert.That(obsolete, Is.Not.Null);
		Assert.That(obsolete.IsError, Is.False);
		Assert.That(obsolete.Message, Does.Contain(replacement));
	}

	/// <summary>
	/// The restored property remains independent storage and does not change
	/// the column flags used by schema writers.
	/// </summary>
	[Test]
	public void PrimaryKeyPreservesLegacyStorageWithoutChangingColumnFlags()
	{
		Table table = SchemaFixtures.GetOrdersTable();
		SqlWriter writer = new();
		string originalSql = writer.GetTableCreateStatement(table);

#pragma warning disable CS0618 // Verify the retained published property.
		Assert.That(table.PrimaryKey, Is.Null);
		table.PrimaryKey = "productId";
		Assert.That(table.PrimaryKey, Is.EqualTo("productId"));
		Assert.That(table.Columns["id"].Primary, Is.True);
		Assert.That(table.Columns["productId"].Primary, Is.False);
		Assert.That(writer.GetTableCreateStatement(table), Is.EqualTo(originalSql));
		table.PrimaryKey = null;
		Assert.That(table.PrimaryKey, Is.Null);
#pragma warning restore CS0618

		PropertyInfo property = typeof(Table).GetProperty("PrimaryKey");
		Assert.That(property.GetMethod.IsPublic, Is.True);
		Assert.That(property.SetMethod.IsPublic, Is.True);
		ObsoleteAttribute obsolete =
			property.GetCustomAttribute<ObsoleteAttribute>();
		Assert.That(obsolete, Is.Not.Null);
		Assert.That(obsolete.IsError, Is.False);
		Assert.That(obsolete.Message, Does.Contain("Column.Primary"));
	}

	/// <summary>
	/// The restored constructor retains its original base initialization and
	/// OLE DB storage replacement, without opening a connection.
	/// </summary>
	[SupportedOSPlatform("windows")]
	[Test]
	public void LegacyConstructorPreservesBaseMetadataAndOleDbStorage()
	{
		string databaseFilePath = Path.GetTempFileName();
		try
		{
#pragma warning disable CS0618 // Verify the retained published constructor.
			using DataObjectOleDb item = new(
				databaseType: DatabaseType.SQLite,
				databaseFilePath: databaseFilePath);
#pragma warning restore CS0618
			Assert.That(item.DatabaseType, Is.EqualTo(DatabaseType.SQLite));
			Assert.That(item.DatabaseFilePath, Is.EqualTo(databaseFilePath));
			Assert.That(item.Database, Is.InstanceOf<DataStorageOleDb>());
			Assert.That(
				item.Database.ConnectionText,
				Is.EqualTo(OleDbHelper.BuildConnectionString(databaseFilePath)));
		}
		finally
		{
			File.Delete(databaseFilePath);
		}

		ConstructorInfo constructor = typeof(DataObjectOleDb).GetConstructor(
			[typeof(DatabaseType), typeof(string)]);
		ObsoleteAttribute obsolete =
			constructor.GetCustomAttribute<ObsoleteAttribute>();
		Assert.That(obsolete, Is.Not.Null);
		Assert.That(obsolete.IsError, Is.False);
		Assert.That(obsolete.Message, Does.Contain("DataObjectOleDb(string"));
	}

	/// <summary>
	/// Restoring the old constructor does not silently change the database
	/// types rejected by its historical base-constructor path.
	/// </summary>
	/// <param name="databaseType">A type the base constructor rejects.</param>
	[SupportedOSPlatform("windows")]
	[TestCase(DatabaseType.OleDb)]
	[TestCase(DatabaseType.Unknown)]
	public void LegacyConstructorPreservesUnsupportedDatabaseTypeFailure(
		DatabaseType databaseType)
	{
#pragma warning disable CS0618 // Preserve the published failure behavior.
		Assert.Throws<NotImplementedException>(
			() => new DataObjectOleDb(databaseType, "unused.accdb"));
#pragma warning restore CS0618
	}

	/// <summary>
	/// The exact two-argument method binds to the old delegate signature and
	/// retains Access output while the explicit SQLite option still works.
	/// </summary>
	[SupportedOSPlatform("windows")]
	[NonParallelizable]
	[Test]
	public void TwoArgumentExportSchemaRetainsDefaultAccessOutput()
	{
		Func<string, Collection<Table>> originalLoader =
			DataDefinitionOleDb.SchemaLoader;
		string directory = Path.Combine(
			Path.GetTempPath(), "LegacyExportTests-" + Guid.NewGuid());
		Directory.CreateDirectory(directory);

		try
		{
			Table table = new("Flags");
			Column id = new("id", ColumnType.AutoNumber, 0, false, false, null, 1);
			id.Primary = true;
			table.AddColumn(id);
			table.AddColumn(new Column(
				"enabled", ColumnType.YesNo, 0, false, false, null, 2));
			DataDefinitionOleDb.SchemaLoader = _ => [table];

			Func<string, string, bool> legacyExport =
				DataDefinitionOleDb.ExportSchema;
			string legacyPath = Path.Combine(directory, "legacy.sql");
			string accessPath = Path.Combine(directory, "access.sql");
			string sqlitePath = Path.Combine(directory, "sqlite.sql");

			Assert.That(legacyExport("ignored.accdb", legacyPath), Is.True);
			bool accessExported = DataDefinitionOleDb.ExportSchema(
				"ignored.accdb", accessPath, toSqlite: false);
			bool sqliteExported = DataDefinitionOleDb.ExportSchema(
				"ignored.accdb", sqlitePath, toSqlite: true);
			Assert.That(accessExported, Is.True);
			Assert.That(sqliteExported, Is.True);
			string legacySql = File.ReadAllText(legacyPath);
			Assert.That(legacySql, Is.EqualTo(File.ReadAllText(accessPath)));
			Assert.That(legacySql, Does.Contain("[enabled] BIT"));
			Assert.That(File.ReadAllText(sqlitePath), Does.Contain("\"enabled\" INTEGER"));
		}
		finally
		{
			DataDefinitionOleDb.SchemaLoader = originalLoader;
			Directory.Delete(directory, true);
		}
	}
}
