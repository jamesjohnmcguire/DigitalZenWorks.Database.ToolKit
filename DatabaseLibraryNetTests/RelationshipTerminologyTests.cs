// <copyright file="RelationshipTerminologyTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using NUnit.Framework;

/// <summary>
/// Verifies explicit relationship roles and the existing public API contract.
/// </summary>
[TestFixture]
internal sealed class RelationshipTerminologyTests
{
	/// <summary>
	/// The factory accepts the referenced parent first without reversing the
	/// historical storage expected by existing consumers.
	/// </summary>
	[Test]
	public void CreateUsesParentFirstAndPreservesLegacyStorage()
	{
		ForeignKey key = ForeignKey.Create(
			"FK_Orders_Products",
			"Products",
			"id",
			"Orders",
			"productId",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade);

		Assert.That(key.Name, Is.EqualTo("FK_Orders_Products"));
		Assert.That(key.ReferencedTable, Is.EqualTo("Products"));
		Assert.That(key.ReferencedColumn, Is.EqualTo("id"));
		Assert.That(key.ReferencingTable, Is.EqualTo("Orders"));
		Assert.That(key.ReferencingColumn, Is.EqualTo("productId"));
		Assert.That(key.ChildTable, Is.EqualTo("Products"));
		Assert.That(key.ChildColumn, Is.EqualTo("id"));
		Assert.That(key.ParentTable, Is.EqualTo("Orders"));
		Assert.That(key.ParentColumn, Is.EqualTo("productId"));
		Assert.That(key.OnDeleteAction, Is.EqualTo(ConstraintAction.SetNull));
		Assert.That(key.OnUpdateAction, Is.EqualTo(ConstraintAction.Cascade));
	}

	/// <summary>
	/// Factory validation rejects missing identifiers with the correct
	/// parameter name while leaving legacy constructors permissive.
	/// </summary>
	/// <param name="parameterName">The identifier to invalidate.</param>
	/// <param name="value">The invalid identifier.</param>
	[TestCase("name", null)]
	[TestCase("name", "")]
	[TestCase("name", " \t")]
	[TestCase("referencedTable", null)]
	[TestCase("referencedTable", "")]
	[TestCase("referencedTable", " \t")]
	[TestCase("referencedColumn", null)]
	[TestCase("referencedColumn", "")]
	[TestCase("referencedColumn", " \t")]
	[TestCase("referencingTable", null)]
	[TestCase("referencingTable", "")]
	[TestCase("referencingTable", " \t")]
	[TestCase("referencingColumn", null)]
	[TestCase("referencingColumn", "")]
	[TestCase("referencingColumn", " \t")]
	public void CreateRejectsMissingIdentifiers(
		string parameterName, string value)
	{
		Dictionary<string, string> identifiers = new()
		{
			["name"] = "FK_Orders_Products",
			["referencedTable"] = "Products",
			["referencedColumn"] = "id",
			["referencingTable"] = "Orders",
			["referencingColumn"] = "productId",
		};
		identifiers[parameterName] = value;

		ArgumentException exception = Assert.Catch<ArgumentException>(
			() => ForeignKey.Create(
				identifiers["name"],
				identifiers["referencedTable"],
				identifiers["referencedColumn"],
				identifiers["referencingTable"],
				identifiers["referencingColumn"],
				ConstraintAction.NoAction,
				ConstraintAction.NoAction));

		Type expectedType = value == null
			? typeof(ArgumentNullException)
			: typeof(ArgumentException);
		Assert.That(exception.GetType(), Is.EqualTo(expectedType));
		Assert.That(exception.ParamName, Is.EqualTo(parameterName));
	}

	/// <summary>
	/// New and historical property names share storage in both directions,
	/// including null assignments allowed by the current API.
	/// </summary>
	/// <param name="modelType">The public relationship model.</param>
	/// <param name="explicitName">The explicit role property.</param>
	/// <param name="legacyName">Its historical alias.</param>
	[TestCase(typeof(ForeignKey), "ReferencedTable", "ChildTable")]
	[TestCase(typeof(ForeignKey), "ReferencedColumn", "ChildColumn")]
	[TestCase(typeof(ForeignKey), "ReferencedColumn", "ColumnName")]
	[TestCase(typeof(ForeignKey), "ReferencingTable", "ParentTable")]
	[TestCase(typeof(ForeignKey), "ReferencingColumn", "ParentColumn")]
	[TestCase(typeof(ForeignKey), "ReferencingColumn", "ParentTableColumn")]
	[TestCase(typeof(Relationship), "ReferencedTable", "ChildTable")]
	[TestCase(typeof(Relationship), "ReferencedColumn", "ChildTableCol")]
	[TestCase(typeof(Relationship), "ReferencingTable", "ParentTable")]
	[TestCase(typeof(Relationship), "ReferencingColumn", "ParentTableCol")]
	public void ExplicitAndLegacyPropertiesStaySynchronized(
		Type modelType, string explicitName, string legacyName)
	{
		object model = modelType == typeof(ForeignKey)
			? new ForeignKey(
				null,
				null,
				null,
				null,
				null,
				ConstraintAction.NoAction,
				ConstraintAction.NoAction)
			: new Relationship();
		PropertyInfo explicitProperty = modelType.GetProperty(explicitName);
		PropertyInfo legacyProperty = modelType.GetProperty(legacyName);

		legacyProperty.SetValue(model, "legacy value");
		Assert.That(explicitProperty.GetValue(model), Is.EqualTo("legacy value"));
		explicitProperty.SetValue(model, "explicit value");
		Assert.That(legacyProperty.GetValue(model), Is.EqualTo("explicit value"));

		explicitProperty.SetValue(model, null);
		Assert.That(legacyProperty.GetValue(model), Is.Null);
		legacyProperty.SetValue(model, null);
		Assert.That(explicitProperty.GetValue(model), Is.Null);
	}

	/// <summary>
	/// Existing named constructor arguments retain their order and meanings.
	/// </summary>
	[Test]
	public void CompleteConstructorRetainsNamedArgumentContract()
	{
		ForeignKey key = new(
			name: "FK_Orders_Products",
			parentTable: "Orders",
			parentColumn: "productId",
			childTable: "Products",
			childColumn: "id",
			onDeleteAction: ConstraintAction.SetNull,
			onUpdateAction: ConstraintAction.Cascade);

		Assert.That(key.ReferencingTable, Is.EqualTo("Orders"));
		Assert.That(key.ReferencingColumn, Is.EqualTo("productId"));
		Assert.That(key.ReferencedTable, Is.EqualTo("Products"));
		Assert.That(key.ReferencedColumn, Is.EqualTo("id"));
	}

	/// <summary>
	/// The shorter constructor remains callable with unchanged assignments
	/// and warns consumers about removal in v2.
	/// </summary>
	[Test]
	public void ShortConstructorIsObsoleteWithoutChangingAssignments()
	{
#pragma warning disable CS0618 // Verify the legacy compatibility contract.
		ForeignKey key = new(
			name: "FK_Orders_Products",
			columnName: "productId",
			parentTable: "Products",
			parentTableColumn: "id",
			onDeleteAction: ConstraintAction.SetNull,
			onUpdateAction: ConstraintAction.Cascade);
#pragma warning restore CS0618

		Assert.That(key.ChildColumn, Is.EqualTo("productId"));
		Assert.That(key.ParentTable, Is.EqualTo("Products"));
		Assert.That(key.ParentColumn, Is.EqualTo("id"));
		Assert.That(key.ChildTable, Is.Null);
		Assert.That(key.OnDeleteAction, Is.EqualTo(ConstraintAction.SetNull));
		Assert.That(key.OnUpdateAction, Is.EqualTo(ConstraintAction.Cascade));

		ConstructorInfo constructor = typeof(ForeignKey).GetConstructor(
			[
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(string),
				typeof(ConstraintAction),
				typeof(ConstraintAction),
			]);
		ObsoleteAttribute attribute =
			constructor.GetCustomAttribute<ObsoleteAttribute>();
		Assert.That(attribute, Is.Not.Null);
		Assert.That(attribute.IsError, Is.False);
		Assert.That(attribute.Message, Does.Contain("ForeignKey.Create"));
		Assert.That(attribute.Message, Does.Contain("v2"));
	}

	/// <summary>
	/// Provider metadata is converted without introducing factory validation
	/// into the existing permissive schema-reader API.
	/// </summary>
	[Test]
	public void RelationshipConversionPreservesRolesAndUnnamedConstraints()
	{
		Relationship relationship = new()
		{
			Name = null,
			ReferencedTable = "Products",
			ReferencedColumn = "id",
			ReferencingTable = "Orders",
			ReferencingColumn = "productId",
			OnDeleteAction = ConstraintAction.SetNull,
			OnUpdateAction = ConstraintAction.Cascade,
		};
		ForeignKey[] keys =
			DataStoreStructure.GetForeignKeyRelationships([relationship]);

		Assert.That(keys.Length, Is.EqualTo(1));
		Assert.That(keys[0].Name, Is.Null);
		Assert.That(keys[0].ReferencedTable, Is.EqualTo("Products"));
		Assert.That(keys[0].ReferencedColumn, Is.EqualTo("id"));
		Assert.That(keys[0].ReferencingTable, Is.EqualTo("Orders"));
		Assert.That(keys[0].ReferencingColumn, Is.EqualTo("productId"));
		Assert.That(keys[0].OnDeleteAction, Is.EqualTo(ConstraintAction.SetNull));
		Assert.That(keys[0].OnUpdateAction, Is.EqualTo(ConstraintAction.Cascade));
	}

	/// <summary>
	/// Each SQL writer preserves the SQL produced by existing constructor
	/// calls when the same relationship is created with explicit roles.
	/// </summary>
	/// <param name="writerType">The writer implementation.</param>
	[TestCase(typeof(SqlWriter))]
	[TestCase(typeof(SqlWriterOleDb))]
	[TestCase(typeof(SqlWriterSqlite))]
	public void FactoryAndLegacyConstructorGenerateIdenticalSql(Type writerType)
	{
		SqlWriter writer = (SqlWriter)Activator.CreateInstance(writerType);
		Table legacyTable = SchemaFixtures.GetOrdersTable();
		legacyTable.ForeignKeys.Clear();
		legacyTable.ForeignKeys.Add(new ForeignKey(
			"FK_Orders_0_0",
			"Orders",
			"productId",
			"Products",
			"id",
			ConstraintAction.SetNull,
			ConstraintAction.Cascade));
		Table explicitTable = SchemaFixtures.GetOrdersTable();

		string legacySql = writer.GetTableCreateStatement(legacyTable);
		Assert.That(
			writer.GetTableCreateStatement(explicitTable), Is.EqualTo(legacySql));
	}

	/// <summary>
	/// A referencing table is ordered after its referenced prerequisite even
	/// when the input collection lists the child first.
	/// </summary>
	[Test]
	public void OrderTablesPlacesReferencedParentBeforeReferencingChild()
	{
		Table child = SchemaFixtures.GetContactsTable();
		Table parent = SchemaFixtures.GetAddressesTable();
		Collection<Table> ordered = DataStoreStructure.OrderTables([child, parent]);

		Assert.That(ordered.Count, Is.EqualTo(2));
		Assert.That(ordered[0], Is.SameAs(parent));
		Assert.That(ordered[1], Is.SameAs(child));
	}
}
