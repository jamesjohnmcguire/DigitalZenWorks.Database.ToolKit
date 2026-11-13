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
