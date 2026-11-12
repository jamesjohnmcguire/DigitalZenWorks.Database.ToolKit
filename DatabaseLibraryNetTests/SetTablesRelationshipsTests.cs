// <copyright file="SetTablesRelationshipsTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Reflection;
using NUnit.Framework;

#pragma warning disable CS0618 // Exercise the retained relationship API.

/// <summary>
/// Protects behavior shared by the legacy and foreign-key overloads.
/// </summary>
[TestFixture]
internal sealed class SetTablesRelationshipsTests
{
	/// <summary>
	/// Both paths retain existing keys, append in order, and use the child.
	/// </summary>
	/// <param name="legacy">Whether to use the relationship overload.</param>
	[TestCase(true)]
	[TestCase(false)]
	public void AddsKeysToReferencingTableAndPreservesMetadata(bool legacy)
	{
		using SchemaProbe schema = new();
		Table child = new("Orders");
		Table parent = new("Products");
		Dictionary<string, Table> tables = new()
		{
			[child.Name] = child,
			[parent.Name] = parent,
		};
		ForeignKey existing = CreateKey("Existing");
		ForeignKey key = CreateKey("FK_Orders_Products");
		child.ForeignKeys.Add(existing);

		Dictionary<string, Table> result =
			schema.Apply(tables, [null, key, key], legacy);

		Assert.That(result, Is.SameAs(tables));
		Assert.That(result[child.Name], Is.SameAs(child));
		Assert.That(child.ForeignKeys.Count, Is.EqualTo(3));
		Assert.That(child.ForeignKeys[0], Is.SameAs(existing));
		Assert.That(parent.ForeignKeys, Is.Empty);
		foreach (ForeignKey added in new[]
		{
			child.ForeignKeys[1], child.ForeignKeys[2],
		})
		{
			Assert.That(added.Name, Is.EqualTo("FK_Orders_Products"));
			Assert.That(added.ReferencingTable, Is.EqualTo("Orders"));
			Assert.That(added.ReferencingColumn, Is.EqualTo("productId"));
			Assert.That(added.ReferencedTable, Is.EqualTo("Products"));
			Assert.That(added.ReferencedColumn, Is.EqualTo("id"));
			Assert.That(
				added.OnDeleteAction, Is.EqualTo(ConstraintAction.SetNull));
			Assert.That(
				added.OnUpdateAction, Is.EqualTo(ConstraintAction.Cascade));
		}
	}

	/// <summary>
	/// Null dictionaries fail even when the supplied keys are null.
	/// </summary>
	/// <param name="legacy">Whether to use the relationship overload.</param>
	[TestCase(true)]
	[TestCase(false)]
	public void NullDictionaryThrowsWithParameterName(bool legacy)
	{
		using SchemaProbe schema = new();
		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
			() => schema.Apply(null, null, legacy));
		Assert.That(exception.ParamName, Is.EqualTo("tableDictionary"));
		Assert.That(
			exception.Message, Does.Contain("Table dictionary cannot be null"));
	}

	/// <summary>
	/// Null collections and empty collections leave the original keys intact.
	/// </summary>
	/// <param name="legacy">Whether to use the relationship overload.</param>
	/// <param name="nullCollection">Whether the collection is null.</param>
	[TestCase(true, true)]
	[TestCase(true, false)]
	[TestCase(false, true)]
	[TestCase(false, false)]
	public void NoKeysLeavesDictionaryUnchanged(
		bool legacy, bool nullCollection)
	{
		using SchemaProbe schema = new();
		Table child = new("Orders");
		ForeignKey existing = CreateKey("Existing");
		child.ForeignKeys.Add(existing);
		Dictionary<string, Table> tables = new() { [child.Name] = child };
		Collection<ForeignKey> keys = nullCollection ? null : [];

		Assert.That(schema.Apply(tables, keys, legacy), Is.SameAs(tables));
		Assert.That(child.ForeignKeys, Is.EqualTo(new[] { existing }));
	}

	/// <summary>
	/// A missing child fails after earlier valid keys have been applied.
	/// </summary>
	/// <param name="legacy">Whether to use the relationship overload.</param>
	[TestCase(true)]
	[TestCase(false)]
	public void MissingReferencingTablePreservesEarlierChanges(bool legacy)
	{
		using SchemaProbe schema = new();
		Table child = new("Orders");
		Dictionary<string, Table> tables = new() { [child.Name] = child };
		ForeignKey missing = CreateKey("Missing");
		missing.ReferencingTable = "Unknown";

		Assert.Throws<KeyNotFoundException>(() => schema.Apply(
			tables, [CreateKey("First"), missing, CreateKey("Last")], legacy));
		Assert.That(child.ForeignKeys.Count, Is.EqualTo(1));
		Assert.That(child.ForeignKeys[0].Name, Is.EqualTo("First"));
	}

	/// <summary>
	/// Existing provider metadata is not subjected to factory validation.
	/// </summary>
	/// <param name="legacy">Whether to use the relationship overload.</param>
	[TestCase(true)]
	[TestCase(false)]
	public void IncompleteMetadataRetainsPermissiveBehavior(bool legacy)
	{
		using SchemaProbe schema = new();
		Table child = new("Orders");
		Dictionary<string, Table> tables = new() { [child.Name] = child };
		ForeignKey key = new(
			null,
			"Orders",
			null,
			null,
			null,
			ConstraintAction.NoAction,
			ConstraintAction.NoAction);

		schema.Apply(tables, [key], legacy);

		Assert.That(child.ForeignKeys.Count, Is.EqualTo(1));
		Assert.That(child.ForeignKeys[0].Name, Is.Null);
		Assert.That(child.ForeignKeys[0].ReferencingColumn, Is.Null);
		Assert.That(child.ForeignKeys[0].ReferencedTable, Is.Null);
		Assert.That(child.ForeignKeys[0].ReferencedColumn, Is.Null);
	}

	/// <summary>
	/// The new overload attaches the supplied object without a lossy copy.
	/// </summary>
	[Test]
	public void ForeignKeyOverloadPreservesObjectIdentity()
	{
		using SchemaProbe schema = new();
		Table child = new("Orders");
		Dictionary<string, Table> tables = new() { [child.Name] = child };
		ForeignKey key = CreateKey("Original");

		schema.Apply(tables, [key], false);
		Assert.That(child.ForeignKeys[0], Is.SameAs(key));
	}

	/// <summary>
	/// Retained callers continue to dispatch through the virtual hook.
	/// </summary>
	[Test]
	public void LegacyOverloadRetainsVirtualHook()
	{
		using SchemaProbe schema = new();
		Table child = new("Orders");
		Dictionary<string, Table> tables = new() { [child.Name] = child };

		schema.Apply(tables, [null, CreateKey("Key")], true);

		Assert.That(schema.RelationshipCalls, Is.EqualTo(2));
		Assert.That(child.ForeignKeys.Count, Is.EqualTo(1));
	}

	/// <summary>
	/// The legacy overload remains callable with actionable migration advice.
	/// </summary>
	[Test]
	public void LegacyOverloadIsObsoleteWithMigrationGuidance()
	{
		MethodInfo method = typeof(DataStoreStructure).GetMethod(
			"SetTablesRelationships",
			BindingFlags.Instance | BindingFlags.NonPublic,
			null,
			[
				typeof(Dictionary<string, Table>),
				typeof(Collection<Relationship>),
			],
			null);
		ObsoleteAttribute attribute =
			method.GetCustomAttribute<ObsoleteAttribute>();

		Assert.That(attribute, Is.Not.Null);
		Assert.That(attribute.IsError, Is.False);
		Assert.That(attribute.Message, Does.Contain("Collection<ForeignKey>"));
	}

	/// <summary>
	/// Schema loading retains provider decoding and skips legacy attachment.
	/// </summary>
	/// <param name="nullForeignKey">Whether the provider returns null.</param>
	[TestCase(false)]
	[TestCase(true)]
	public void GetSchemaUsesForeignKeysWithoutLegacyAttachment(
		bool nullForeignKey)
	{
		using SchemaProbe schema = new()
		{
			ReturnNullForeignKey = nullForeignKey,
		};

		Collection<Table> tables = schema.GetSchema();

		Assert.That(tables.Count, Is.EqualTo(2));
		Assert.That(tables[0].Name, Is.EqualTo("Orders"));
		Assert.That(tables[1].Name, Is.EqualTo("Products"));
		Assert.That(tables[1].ForeignKeys, Is.Empty);
		Assert.That(schema.DecodingCalls, Is.EqualTo(1));
		Assert.That(schema.LegacyDecodingCalls, Is.Zero);
		Assert.That(schema.RelationshipCalls, Is.Zero);
		if (nullForeignKey)
		{
			Assert.That(tables[0].ForeignKeys, Is.Empty);
		}
		else
		{
			Assert.That(tables[0].ForeignKeys.Count, Is.EqualTo(1));
			ForeignKey key = tables[0].ForeignKeys[0];
			Assert.That(key.Name, Is.EqualTo("FK_Orders_Products"));
			Assert.That(key.ReferencingTable, Is.EqualTo("Orders"));
			Assert.That(key.ReferencingColumn, Is.EqualTo("productId"));
			Assert.That(key.ReferencedTable, Is.EqualTo("Products"));
			Assert.That(key.ReferencedColumn, Is.EqualTo("id"));
			Assert.That(
				key.OnDeleteAction, Is.EqualTo(ConstraintAction.SetNull));
			Assert.That(
				key.OnUpdateAction, Is.EqualTo(ConstraintAction.Cascade));
		}
	}

	/// <summary>
	/// The deprecated hook remains virtual and offers migration guidance.
	/// </summary>
	[Test]
	public void LegacyAttachmentHookIsObsoleteAndRemainsVirtual()
	{
		MethodInfo method = typeof(DataStoreStructure).GetMethod(
			"GetTableWithRelationships",
			BindingFlags.Instance | BindingFlags.NonPublic);
		ObsoleteAttribute attribute =
			method.GetCustomAttribute<ObsoleteAttribute>();

		Assert.That(method.IsVirtual, Is.True);
		Assert.That(attribute, Is.Not.Null);
		Assert.That(attribute.IsError, Is.False);
		Assert.That(attribute.Message, Does.Contain("Collection<ForeignKey>"));
	}

	/// <summary>
	/// The collection overload appends without replacing existing entries.
	/// </summary>
	[Test]
	public void GetForeignKeysAppendsToExistingCollection()
	{
		using SchemaProbe schema = new();
		ForeignKey existing = CreateKey("Existing");
		Collection<ForeignKey> keys = [existing];

		Assert.That(schema.GetForeignKeys("Orders", keys), Is.SameAs(keys));
		Assert.That(keys.Count, Is.EqualTo(2));
		Assert.That(keys[0], Is.SameAs(existing));
		Assert.That(keys[1].Name, Is.EqualTo("FK_Orders_Products"));
		Assert.That(schema.GetForeignKeys("Products", keys), Is.SameAs(keys));
		Assert.That(keys.Count, Is.EqualTo(2));
	}

	/// <summary>
	/// A null destination creates a new collection, including for empty data.
	/// </summary>
	/// <param name="tableName">The requested table.</param>
	/// <param name="expectedCount">The expected number of keys.</param>
	[TestCase("Orders", 1)]
	[TestCase("Products", 0)]
	public void GetForeignKeysCreatesCollection(
		string tableName, int expectedCount)
	{
		using SchemaProbe schema = new();
		Collection<ForeignKey> keys = schema.GetForeignKeys(tableName, null);

		Assert.That(keys, Is.Not.Null);
		Assert.That(keys.Count, Is.EqualTo(expectedCount));
	}

	/// <summary>
	/// Action decoding preserves the existing case-sensitive provider rules.
	/// </summary>
	/// <param name="deleteRule">The delete rule text.</param>
	/// <param name="updateRule">The update rule text.</param>
	/// <param name="deleteAction">The expected delete action.</param>
	/// <param name="updateAction">The expected update action.</param>
	[TestCase(
		"CASCADE",
		"SET NULL",
		ConstraintAction.Cascade,
		ConstraintAction.SetNull)]
	[TestCase(
		"SET NULL",
		"CASCADE",
		ConstraintAction.SetNull,
		ConstraintAction.Cascade)]
	[TestCase(
		"NO ACTION",
		"RESTRICT",
		ConstraintAction.NoAction,
		ConstraintAction.NoAction)]
	[TestCase(
		"cascade",
		"set null",
		ConstraintAction.NoAction,
		ConstraintAction.NoAction)]
	[TestCase("", "", ConstraintAction.NoAction, ConstraintAction.NoAction)]
	public void ForeignKeyDecodingPreservesActionRules(
		string deleteRule,
		string updateRule,
		ConstraintAction deleteAction,
		ConstraintAction updateAction)
	{
		using SchemaProbe schema = new();
		using DataTable metadata = schema.GetForeignKeys("Orders");
		DataRow row = metadata.Rows[0];
		row["FKEY_ON_DELETE"] = deleteRule;
		row["FKEY_ON_UPDATE"] = updateRule;
		row["CONSTRAINT_NAME"] = DBNull.Value;

		ForeignKey key = schema.ReadForeignKey(row);

		Assert.That(key.Name, Is.Empty);
		Assert.That(key.OnDeleteAction, Is.EqualTo(deleteAction));
		Assert.That(key.OnUpdateAction, Is.EqualTo(updateAction));
	}

	/// <summary>
	/// Null metadata is rejected with the correct parameter name.
	/// </summary>
	[Test]
	public void ForeignKeyDecodingRejectsNullRow()
	{
		using SchemaProbe schema = new();
		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
			() => schema.ReadForeignKey(null));
		Assert.That(exception.ParamName, Is.EqualTo("row"));
	}

	/// <summary>
	/// Creates a key whose referenced and referencing names differ.
	/// </summary>
	/// <param name="name">The constraint name.</param>
	/// <returns>The foreign key.</returns>
	private static ForeignKey CreateKey(string name)
	{
		return ForeignKey.Create(
			name,
			"Products",
			"id",
			"Orders",
			"productId",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade);
	}

	/// <summary>
	/// Exposes protected overloads through a real in-memory database reader.
	/// </summary>
	private sealed class SchemaProbe : DataStoreStructure
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SchemaProbe"/> class.
		/// </summary>
		public SchemaProbe()
			: base(DatabaseType.SQLite, ":memory:")
		{
		}

		/// <summary>
		/// Gets the number of calls to the legacy virtual hook.
		/// </summary>
		public int RelationshipCalls { get; private set; }

		/// <summary>
		/// Gets or sets a value indicating whether provider decoding returns
		/// null metadata.
		/// </summary>
		public bool ReturnNullForeignKey { get; set; }

		/// <summary>
		/// Gets the number of provider decoding calls.
		/// </summary>
		public int DecodingCalls { get; private set; }

		/// <summary>
		/// Gets the number of legacy decoding calls.
		/// </summary>
		public int LegacyDecodingCalls { get; private set; }

		/// <inheritdoc/>
		public override DataTable TableNames
		{
			get
			{
				DataTable names = new();
				names.Columns.Add("TABLE_NAME", typeof(string));
				names.Rows.Add("Orders");
				names.Rows.Add("Products");
				return names;
			}
		}

		/// <inheritdoc/>
		public override Table GetTable(string tableName)
		{
			return new Table(tableName);
		}

		/// <inheritdoc/>
		public override DataTable GetForeignKeys(string tableName)
		{
			DataTable keys = new();
			foreach (string column in new[]
			{
				"CONSTRAINT_NAME", "TABLE_NAME", "FKEY_FROM_COLUMN",
				"FKEY_TO_TABLE", "FKEY_TO_COLUMN", "FKEY_ON_DELETE",
				"FKEY_ON_UPDATE",
			})
			{
				keys.Columns.Add(column, typeof(string));
			}

			if (tableName == "Orders")
			{
				keys.Rows.Add(
					"FK_Orders_Products",
					tableName,
					"productId",
					"Products",
					"id",
					"SET NULL",
					"CASCADE");
			}

			return keys;
		}

		/// <summary>
		/// Applies the same metadata through either overload.
		/// </summary>
		/// <param name="tables">The table dictionary.</param>
		/// <param name="keys">The foreign keys.</param>
		/// <param name="legacy">Whether to use the legacy overload.</param>
		/// <returns>The original table dictionary.</returns>
		public Dictionary<string, Table> Apply(
			Dictionary<string, Table> tables,
			Collection<ForeignKey> keys,
			bool legacy)
		{
			if (!legacy)
			{
				return SetTablesRelationships(tables, keys);
			}

			Collection<Relationship> relationships = null;
			if (keys != null)
			{
				relationships = [];
				foreach (ForeignKey key in keys)
				{
					Relationship relationship = null;
					if (key != null)
					{
						relationship = new()
						{
							Name = key.Name,
							ReferencingTable = key.ReferencingTable,
							ReferencingColumn = key.ReferencingColumn,
							ReferencedTable = key.ReferencedTable,
							ReferencedColumn = key.ReferencedColumn,
							OnDeleteAction = key.OnDeleteAction,
							OnUpdateAction = key.OnUpdateAction,
						};
					}

					relationships.Add(relationship);
				}
			}

			return SetTablesRelationships(tables, relationships);
		}

		/// <summary>
		/// Exposes provider decoding for metadata tests.
		/// </summary>
		/// <param name="row">The metadata row.</param>
		/// <returns>The foreign key.</returns>
		public ForeignKey ReadForeignKey(DataRow row)
		{
			return GetForeignKey(row);
		}

		/// <inheritdoc/>
		protected override ForeignKey GetForeignKey(DataRow row)
		{
			DecodingCalls++;
			if (ReturnNullForeignKey)
			{
				return null;
			}

			return base.GetForeignKey(row);
		}

		/// <inheritdoc/>
		protected override Relationship GetRelationship(DataRow foreignKey)
		{
			LegacyDecodingCalls++;
			throw new InvalidOperationException(
				"Schema loading must not use Relationship decoding.");
		}

		/// <inheritdoc/>
		[Obsolete("Use the Collection<ForeignKey> overload instead.")]
		protected override Table GetTableWithRelationships(
			Dictionary<string, Table> tableDictionary,
			Relationship relationship)
		{
			RelationshipCalls++;
			return base.GetTableWithRelationships(
				tableDictionary, relationship);
		}
	}
}

#pragma warning restore CS0618
