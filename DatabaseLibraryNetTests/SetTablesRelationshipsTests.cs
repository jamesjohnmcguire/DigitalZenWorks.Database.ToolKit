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
/// Protects legacy relationship behavior and foreign-key schema loading.
/// </summary>
[TestFixture]
internal sealed class SetTablesRelationshipsTests
{
	/// <summary>
	/// The legacy path appends keys in order to the referencing table.
	/// </summary>
	[Test]
	public void AddsKeysToReferencingTableAndPreservesMetadata()
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
			schema.ApplyLegacyRelationships(tables, [null, key, key]);

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
	[Test]
	public void NullDictionaryThrowsWithParameterName()
	{
		using SchemaProbe schema = new();
		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
			() => schema.ApplyLegacyRelationships(null, null));
		Assert.That(exception.ParamName, Is.EqualTo("tableDictionary"));
		Assert.That(
			exception.Message, Does.Contain("Table dictionary cannot be null"));
	}

	/// <summary>
	/// Null collections and empty collections leave the original keys intact.
	/// </summary>
	/// <param name="nullCollection">Whether the collection is null.</param>
	[TestCase(true)]
	[TestCase(false)]
	public void NoKeysLeavesDictionaryUnchanged(bool nullCollection)
	{
		using SchemaProbe schema = new();
		Table child = new("Orders");
		ForeignKey existing = CreateKey("Existing");
		child.ForeignKeys.Add(existing);
		Dictionary<string, Table> tables = new() { [child.Name] = child };
		Collection<ForeignKey> keys = nullCollection ? null : [];

		Assert.That(
			schema.ApplyLegacyRelationships(tables, keys), Is.SameAs(tables));
		Assert.That(child.ForeignKeys, Is.EqualTo(new[] { existing }));
	}

	/// <summary>
	/// A missing child fails after earlier valid keys have been applied.
	/// </summary>
	[Test]
	public void MissingReferencingTablePreservesEarlierChanges()
	{
		using SchemaProbe schema = new();
		Table child = new("Orders");
		Dictionary<string, Table> tables = new() { [child.Name] = child };
		ForeignKey missing = CreateKey("Missing");
		missing.ReferencingTable = "Unknown";

		Assert.Throws<KeyNotFoundException>(() =>
			schema.ApplyLegacyRelationships(
				tables, [CreateKey("First"), missing, CreateKey("Last")]));
		Assert.That(child.ForeignKeys.Count, Is.EqualTo(1));
		Assert.That(child.ForeignKeys[0].Name, Is.EqualTo("First"));
	}

	/// <summary>
	/// Existing provider metadata is not subjected to factory validation.
	/// </summary>
	[Test]
	public void IncompleteMetadataRetainsPermissiveBehavior()
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

		schema.ApplyLegacyRelationships(tables, [key]);

		Assert.That(child.ForeignKeys.Count, Is.EqualTo(1));
		Assert.That(child.ForeignKeys[0].Name, Is.Null);
		Assert.That(child.ForeignKeys[0].ReferencingColumn, Is.Null);
		Assert.That(child.ForeignKeys[0].ReferencedTable, Is.Null);
		Assert.That(child.ForeignKeys[0].ReferencedColumn, Is.Null);
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

		schema.ApplyLegacyRelationships(tables, [null, CreateKey("Key")]);

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
		Assert.That(attribute.Message, Does.Contain("Table.ForeignKeys"));
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
		Assert.That(schema.OwnedMetadataCalls, Is.EqualTo(2));
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
		Assert.That(attribute.Message, Does.Contain("Table.ForeignKeys"));
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
	/// Invalid names cannot accidentally query all tables through a provider.
	/// </summary>
	/// <param name="tableName">The invalid table name.</param>
	[TestCase(null)]
	[TestCase("")]
	[TestCase(" \t")]
	public void OwnedForeignKeysRejectMissingTableName(string tableName)
	{
		using SchemaProbe schema = new();
		ArgumentException exception = Assert.Catch<ArgumentException>(
			() => schema.GetForeignKeysOwnedByTable(tableName));

		Assert.That(exception.ParamName, Is.EqualTo("referencingTableName"));
		Type expectedType = tableName == null
			? typeof(ArgumentNullException)
			: typeof(ArgumentException);
		Assert.That(exception.GetType(), Is.EqualTo(expectedType));
		Assert.That(schema.DecodingCalls, Is.Zero);
	}

	/// <summary>
	/// Null metadata results from a provider decoder are ignored.
	/// </summary>
	[Test]
	public void OwnedForeignKeysIgnoreNullDecodedKey()
	{
		using SchemaProbe schema = new() { ReturnNullForeignKey = true };

		Assert.That(schema.GetForeignKeysOwnedByTable("Orders"), Is.Empty);
		Assert.That(schema.DecodingCalls, Is.EqualTo(1));
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
	/// Exposes legacy attachment and metadata hooks for regression tests.
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

		/// <summary>
		/// Gets the number of table ownership metadata queries.
		/// </summary>
		public int OwnedMetadataCalls { get; private set; }

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
		/// Applies metadata through the retained relationship overload.
		/// </summary>
		/// <param name="tables">The table dictionary.</param>
		/// <param name="keys">The foreign keys.</param>
		/// <returns>The original table dictionary.</returns>
		public Dictionary<string, Table> ApplyLegacyRelationships(
			Dictionary<string, Table> tables,
			Collection<ForeignKey> keys)
		{
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
		protected override DataTable GetForeignKeyMetadataOwnedByTable(
			string referencingTableName)
		{
			OwnedMetadataCalls++;
			return base.GetForeignKeyMetadataOwnedByTable(referencingTableName);
		}

		/// <inheritdoc/>
		[Obsolete("Use GetForeignKey instead.")]
		protected override Relationship GetRelationship(DataRow foreignKey)
		{
			LegacyDecodingCalls++;
			throw new InvalidOperationException(
				"Schema loading must not use Relationship decoding.");
		}

		/// <inheritdoc/>
		[Obsolete("Populate Table.ForeignKeys with ForeignKey objects instead.")]
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
