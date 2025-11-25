// <copyright file="DataStoreStructureTests.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using NUnit.Framework;

	/// <summary>
	/// DataStoreStructure tests class.
	/// </summary>
	[TestFixture]
	internal sealed class DataStoreStructureTests : BaseTestsSupport
	{
		/// <summary>
		/// Get constaints test.
		/// </summary>
		[Test]
		public void GetConstraints()
		{
			string tableName = "Sections";

			using DataStoreStructure schema = new (DatabaseType.SQLite, DataSource);

			DataTable constraints =
				schema.GetConstraints(tableName);

			int count = constraints.Rows.Count;

			Assert.That(count, Is.EqualTo(3));

			bool exists = constraints.Columns.Contains("ConstraintName");
			Assert.That(exists, Is.True);

			exists = constraints.Columns.Contains("TableName");
			Assert.That(exists, Is.True);

			exists = constraints.Columns.Contains("ColumnName");
			Assert.That(exists, Is.True);

			IEnumerable<DataRow> dataRows = constraints.Rows.Cast<DataRow>();
			IEnumerable<string> constraintNameStrings =
				dataRows.Select(row => row["ConstraintName"]?.ToString());
			IEnumerable<string> nonEmptyConstraintNames =
				constraintNameStrings.Where(name => !string.IsNullOrEmpty(name));
			List<string> constraintNames = [.. nonEmptyConstraintNames];

			// SQLite names foreign key constraints as FK_<table>_<n>_<m>
			Assert.That(constraintNames, Contains.Item("FK_Sections_0_0"));
			Assert.That(constraintNames, Contains.Item("FK_Sections_1_0"));

#if NOT_SQLITE
			bool hasPrimaryKeyLikeConstraint = constraintNames.Any(
				name => name.StartsWith("Index_", StringComparison.Ordinal));
			Assert.That(hasPrimaryKeyLikeConstraint, Is.True);

			bool hasCategoriesConstraint =
				constraintNames.Any(name => name.Contains(
					"Categories", StringComparison.Ordinal));
			Assert.That(hasCategoriesConstraint, Is.True);
#endif

			foreach (DataRow row in constraints.Rows)
			{
				tableName = row["TableName"]?.ToString();
				Assert.That(tableName, Is.EqualTo("Sections"));
			}
		}
	}
}
