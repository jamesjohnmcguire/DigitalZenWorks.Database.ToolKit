/////////////////////////////////////////////////////////////////////////////
// <copyright file="TableSet.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// Performs in-memory operations on tables and their dependencies.
/// </summary>
internal static class TableSet
{
	/// <summary>
	/// Orders dependency names using the existing depth-first traversal.
	/// </summary>
	/// <param name="tableDependencies">The dependency map, or null.</param>
	/// <returns>The ordered names, or an empty collection for a null map.
	/// Names absent from the map are included when referenced.</returns>
	/// <remarks>Cycles are tolerated. Each name is emitted once, preserving
	/// the encounter order of independent names.</remarks>
	internal static Collection<string> GetOrderedDependencies(
		Dictionary<string, Collection<string>> tableDependencies)
	{
		Collection<string> orderedDependencies = [];

		// Tracks previously processed nodes.
		HashSet<string> visited = [];

		// Tracks nodes in the current recursion stack
		// (for cycle detection).
		HashSet<string> visiting = [];

		if (tableDependencies != null)
		{
			foreach (string key in tableDependencies.Keys)
			{
				if (!visited.Contains(key))
				{
					GetDependenciesRecursive(
						key,
						tableDependencies,
						orderedDependencies,
						visited,
						visiting);
				}
			}
		}

		return orderedDependencies;
	}

	/// <summary>
	/// Orders tables so referenced dependencies precede referencing tables.
	/// </summary>
	/// <param name="tables">The tables to order, or null.</param>
	/// <returns>A new collection containing the original table instances,
	/// or an empty collection for null input. Referenced tables absent
	/// from the input are omitted.</returns>
	/// <remarks>Cycles retain the existing traversal behavior.</remarks>
	internal static Collection<Table> OrderTables(Collection<Table> tables)
	{
		Collection<Table> orderedTables = [];

		if (tables != null)
		{
			Dictionary<string, Collection<string>> tableDependencies = [];
			Dictionary<string, Table> tablesByName = [];

			foreach (Table table in tables)
			{
				string name = table.Name;
				tablesByName[name] = table;

				Collection<string> dependencies = [];

				foreach (ForeignKey foreignKeys in table.ForeignKeys)
				{
					dependencies.Add(foreignKeys.ReferencedTable);
				}

				tableDependencies.Add(name, dependencies);
			}

			Collection<string> orderedNames =
				GetOrderedDependencies(tableDependencies);

			foreach (string tableName in orderedNames)
			{
				if (tablesByName.TryGetValue(tableName, out Table table))
				{
					orderedTables.Add(table);
				}
			}
		}

		return orderedTables;
	}

	/// <summary>
	/// Adds table dependencies to the ordered collection using depth-first
	/// traversal.
	/// </summary>
	/// <param name="key">The table key currently being processed.</param>
	/// <param name="tableDependencies">The dependency map keyed by table
	/// name.</param>
	/// <param name="orderedDependencies">The ordered dependency result.</param>
	/// <param name="visited">The set of table keys already processed.</param>
	/// <param name="visiting">The set of table keys in the active traversal
	/// path.</param>
	private static void GetDependenciesRecursive(
		string key,
		Dictionary<string, Collection<string>> tableDependencies,
		Collection<string> orderedDependencies,
		HashSet<string> visited,
		HashSet<string> visiting)
	{
		if (!visited.Contains(key) && !visiting.Contains(key))
		{
			visiting.Add(key);

			if (tableDependencies.TryGetValue(
				key, out Collection<string> value))
			{
				foreach (string dependency in value)
				{
					GetDependenciesRecursive(
						dependency,
						tableDependencies,
						orderedDependencies,
						visited,
						visiting);
				}
			}

			visiting.Remove(key); // Done visiting
			visited.Add(key);     // Mark as processed
			orderedDependencies.Add(key); // Add to result (postorder)
		}
	}
}
