// <copyright file="TableOrderingTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;

/// <summary>
/// Characterizes both published ordering APIs before their extraction.
/// </summary>
[TestFixture(true)]
[TestFixture(false)]
internal sealed class TableOrderingTests
{
	private readonly bool useDataDefinition;

	/// <summary>
	/// Initializes a new instance of the
	/// <see cref="TableOrderingTests"/> class.
	/// </summary>
	/// <param name="useDataDefinition">Whether to use DataDefinition instead
	/// of DataStoreStructure.</param>
	public TableOrderingTests(bool useDataDefinition)
	{
		this.useDataDefinition = useDataDefinition;
	}

	/// <summary>
	/// Missing dependency maps return independent empty result collections.
	/// </summary>
	/// <param name="nullInput">Whether the map is null rather than empty.
	/// </param>
	[TestCase(true)]
	[TestCase(false)]
	public void NullOrEmptyDependencyMapReturnsFreshEmptyResult(bool nullInput)
	{
		Dictionary<string, Collection<string>> dependencies =
			nullInput ? null : [];

		Collection<string> first = GetDependencyOrder(dependencies);
		Collection<string> second = GetDependencyOrder(dependencies);

		Assert.That(first, Is.Empty);
		Assert.That(second, Is.Empty);
		Assert.That(second, Is.Not.SameAs(first));
	}

	/// <summary>
	/// Dependency traversal handles chains and shared/repeated targets without
	/// mutating the supplied map or its dependency collections.
	/// </summary>
	[Test]
	public void DependenciesPrecedeTablesWithoutChangingInput()
	{
		Collection<string> orderDependencies =
			["Products", "Products", "Customers"];
		Dictionary<string, Collection<string>> dependencies = new()
		{
			["Orders"] = orderDependencies,
			["Customers"] = ["Addresses"],
			["Products"] = ["Categories"],
			["Categories"] = [],
			["Addresses"] = [],
		};
		string[] expected =
			["Categories", "Products", "Addresses", "Customers", "Orders"];
		string[] original = ["Products", "Products", "Customers"];

		Assert.That(GetDependencyOrder(dependencies), Is.EqualTo(expected));
		Assert.That(dependencies.Count, Is.EqualTo(5));
		Assert.That(dependencies["Orders"], Is.SameAs(orderDependencies));
		Assert.That(orderDependencies, Is.EqualTo(original));
	}

	/// <summary>
	/// Independent names retain encounter order and case distinctions.
	/// </summary>
	[Test]
	public void IndependentDependenciesKeepEncounterOrderAndCase()
	{
		Dictionary<string, Collection<string>> dependencies = new()
		{
			["Zulu"] = [],
			["zulu"] = [],
			["Alpha"] = [],
		};
		string[] expected = ["Zulu", "zulu", "Alpha"];

		Assert.That(GetDependencyOrder(dependencies), Is.EqualTo(expected));
	}

	/// <summary>
	/// A dependency on the same table is emitted only once.
	/// </summary>
	[Test]
	public void SelfDependencyAppearsOnce()
	{
		Dictionary<string, Collection<string>> dependencies = new()
		{
			["Categories"] = ["Categories"],
		};
		string[] expected = ["Categories"];

		Assert.That(GetDependencyOrder(dependencies), Is.EqualTo(expected));
	}

	/// <summary>
	/// Cycles retain the existing traversal result rather than throwing.
	/// </summary>
	[Test]
	public void DependencyCycleRetainsCurrentOrder()
	{
		Dictionary<string, Collection<string>> dependencies = new()
		{
			["Orders"] = ["Products"],
			["Products"] = ["Orders"],
		};
		string[] expected = ["Products", "Orders"];

		Assert.That(GetDependencyOrder(dependencies), Is.EqualTo(expected));
	}

	/// <summary>
	/// Referenced names absent from the map are still included in the result.
	/// </summary>
	[Test]
	public void MissingDependencyNameIsIncludedWithoutChangingMap()
	{
		Dictionary<string, Collection<string>> dependencies = new()
		{
			["Orders"] = ["External"],
		};
		string[] expected = ["External", "Orders"];

		Assert.That(GetDependencyOrder(dependencies), Is.EqualTo(expected));
		Assert.That(dependencies.Count, Is.EqualTo(1));
		Assert.That(dependencies.ContainsKey("External"), Is.False);
	}

	/// <summary>
	/// A null dependency collection preserves the existing failure contract.
	/// </summary>
	[Test]
	public void NullDependencyCollectionRetainsFailure()
	{
		Dictionary<string, Collection<string>> dependencies = new()
		{
			["Orders"] = null,
		};

		Assert.Throws<NullReferenceException>(
			() => GetDependencyOrder(dependencies));
	}

	/// <summary>
	/// A null dependency name fails at the dictionary lookup with its key name.
	/// </summary>
	[Test]
	public void NullDependencyNameRetainsFailure()
	{
		Dictionary<string, Collection<string>> dependencies = new()
		{
			["Orders"] = [null],
		};

		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
			() => GetDependencyOrder(dependencies));
		Assert.That(exception.ParamName, Is.EqualTo("key"));
	}

	/// <summary>
	/// Missing table collections return independent empty result collections.
	/// </summary>
	/// <param name="nullInput">Whether the collection is null.</param>
	[TestCase(true)]
	[TestCase(false)]
	public void NullOrEmptyTablesReturnFreshEmptyResult(bool nullInput)
	{
		Collection<Table> tables = nullInput ? null : [];

		Collection<Table> first = GetTableOrder(tables);
		Collection<Table> second = GetTableOrder(tables);

		Assert.That(first, Is.Empty);
		Assert.That(second, Is.Empty);
		Assert.That(second, Is.Not.SameAs(first));
	}

	/// <summary>
	/// Table ordering follows referenced targets, preserves objects, and leaves
	/// the supplied table and foreign-key collections unchanged.
	/// </summary>
	[Test]
	public void TableOrderingPreservesInstancesAndInputCollections()
	{
		Table orders =
			CreateTable("Orders", "Products", "Products", "Customers");
		Table products = CreateTable("Products", "Categories");
		Table customers = CreateTable("Customers");
		Table categories = CreateTable("Categories");
		Collection<Table> input = [orders, customers, products, categories];
		Table[] original = [orders, customers, products, categories];
		Table[] expected = [categories, products, customers, orders];
		ForeignKey originalKey = orders.ForeignKeys[0];

		Collection<Table> result = GetTableOrder(input);

		Assert.That(result, Is.Not.SameAs(input));
		Assert.That(result.Count, Is.EqualTo(expected.Length));
		Assert.That(input.Count, Is.EqualTo(original.Length));
		for (int index = 0; index < expected.Length; index++)
		{
			Assert.That(result[index], Is.SameAs(expected[index]));
			Assert.That(input[index], Is.SameAs(original[index]));
		}

		Assert.That(orders.ForeignKeys.Count, Is.EqualTo(3));
		Assert.That(orders.ForeignKeys[0], Is.SameAs(originalKey));
	}

	/// <summary>
	/// Table results omit dependency names not supplied as table objects.
	/// </summary>
	[Test]
	public void ReferencesOutsideTableCollectionAreOmitted()
	{
		Table orders = CreateTable("Orders", "External");

		Collection<Table> result = GetTableOrder([orders]);

		Assert.That(result.Count, Is.EqualTo(1));
		Assert.That(result[0], Is.SameAs(orders));
	}

	/// <summary>
	/// A self-referencing table remains in the result exactly once.
	/// </summary>
	[Test]
	public void SelfReferencingTableAppearsOnce()
	{
		Table categories = CreateTable("Categories", "Categories");

		Collection<Table> result = GetTableOrder([categories]);

		Assert.That(result.Count, Is.EqualTo(1));
		Assert.That(result[0], Is.SameAs(categories));
	}

	/// <summary>
	/// Mutually referencing tables preserve the existing cycle handling.
	/// </summary>
	[Test]
	public void TableCycleRetainsCurrentOrder()
	{
		Table orders = CreateTable("Orders", "Products");
		Table products = CreateTable("Products", "Orders");

		Collection<Table> result = GetTableOrder([orders, products]);

		Assert.That(result.Count, Is.EqualTo(2));
		Assert.That(result[0], Is.SameAs(products));
		Assert.That(result[1], Is.SameAs(orders));
	}

	/// <summary>
	/// Duplicate table names retain their existing argument failure.
	/// </summary>
	[Test]
	public void DuplicateTableNamesRetainFailure()
	{
		Table first = CreateTable("Orders");
		Table second = CreateTable("Orders");

		Assert.Throws<ArgumentException>(() => GetTableOrder([first, second]));
	}

	/// <summary>
	/// Table names differing only by case remain separate dictionary keys.
	/// </summary>
	[Test]
	public void TableNamesRemainCaseSensitive()
	{
		Table first = CreateTable("Orders");
		Table second = CreateTable("orders");

		Collection<Table> result = GetTableOrder([first, second]);

		Assert.That(result.Count, Is.EqualTo(2));
		Assert.That(result[0], Is.SameAs(first));
		Assert.That(result[1], Is.SameAs(second));
	}

	/// <summary>
	/// Null entries preserve the current table-access failure.
	/// </summary>
	[Test]
	public void NullTableEntryRetainsFailure()
	{
		Assert.Throws<NullReferenceException>(() => GetTableOrder([null]));
	}

	/// <summary>
	/// Null foreign-key entries retain the existing member-access failure.
	/// </summary>
	[Test]
	public void NullForeignKeyEntryRetainsFailure()
	{
		Table orders = CreateTable("Orders");
		orders.ForeignKeys.Add(null);

		Assert.Throws<NullReferenceException>(() => GetTableOrder([orders]));
	}

	/// <summary>
	/// Null referenced names preserve the dictionary key failure.
	/// </summary>
	[Test]
	public void NullReferencedTableRetainsFailure()
	{
		Table orders = CreateTable("Orders", "Products");
		orders.ForeignKeys[0].ReferencedTable = null;

		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(
			() => GetTableOrder([orders]));
		Assert.That(exception.ParamName, Is.EqualTo("key"));
	}

	/// <summary>
	/// Creates a table with foreign keys to the specified referenced tables.
	/// </summary>
	/// <param name="name">The referencing table name.</param>
	/// <param name="referencedTables">The referenced table names.</param>
	/// <returns>The table and its foreign keys.</returns>
	private static Table CreateTable(
		string name, params string[] referencedTables)
	{
		Table table = new(name);
		foreach (string referencedTable in referencedTables)
		{
			ForeignKey key = ForeignKey.Create(
				"FK_" + name + "_" + referencedTable,
				referencedTable,
				"id",
				name,
				"referenceId",
				ConstraintAction.NoAction,
				ConstraintAction.NoAction);
			table.ForeignKeys.Add(key);
		}

		return table;
	}

	/// <summary>
	/// Calls the selected published dependency-ordering entry point.
	/// </summary>
	/// <param name="dependencies">The dependency map.</param>
	/// <returns>The ordered names.</returns>
	private Collection<string> GetDependencyOrder(
		Dictionary<string, Collection<string>> dependencies)
	{
		if (useDataDefinition)
		{
			return DataDefinition.GetOrderedDependencies(dependencies);
		}

		return DataStoreStructure.GetOrderedDependencies(dependencies);
	}

	/// <summary>
	/// Calls the selected published table-ordering entry point.
	/// </summary>
	/// <param name="tables">The tables to order.</param>
	/// <returns>The ordered table instances.</returns>
	private Collection<Table> GetTableOrder(Collection<Table> tables)
	{
		if (useDataDefinition)
		{
			return DataDefinition.OrderTables(tables);
		}

		return DataStoreStructure.OrderTables(tables);
	}
}
