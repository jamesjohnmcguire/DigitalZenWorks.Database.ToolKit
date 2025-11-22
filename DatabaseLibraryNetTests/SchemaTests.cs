// <copyright file="SchemaTests.cs" company="James John McGuire">
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
	/// Schema tests class.
	/// </summary>
	[TestFixture]
	internal sealed class SchemaTests
	{
		/// <summary>
		/// database storage object.
		/// </summary>
		private DataStorage database;
		private string dataSource;

		/// <summary>
		/// One time set up method.
		/// </summary>
		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			dataSource = GetTestDatabasePath();

			SQLiteConnection.CreateFile(dataSource);

			string connectionBase = "Data Source={0};Version=3;" +
				"DateTimeFormat=InvariantCulture";

			string connectionString = string.Format(
				CultureInfo.InvariantCulture,
				connectionBase,
				dataSource);

			database = new DataStorage(DatabaseType.SQLite, connectionString);
		}

		/// <summary>
		/// function that is called when all tests are completed.
		/// </summary>
		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (database != null)
			{
				database.Close();
				database.Shutdown();
			}

			File.Delete(dataSource);
		}

		/// <summary>
		/// Get constaints test.
		/// </summary>
		[Test]
		public void GetConstraints()
		{
			string tableName = "Sections";

			using Schema schema = new (DatabaseType.SQLite, dataSource);

			DataTable constraints =
				schema.GetConstraints(tableName);

			int count = constraints.Rows.Count;

			Assert.That(count, Is.EqualTo(3));

			bool exists = constraints.Columns.Contains("CONSTRAINT_NAME");
			Assert.That(exists, Is.True);

			exists = constraints.Columns.Contains("TABLE_NAME");
			Assert.That(exists, Is.True);

			exists = constraints.Columns.Contains("COLUMN_NAME");
			Assert.That(exists, Is.True);

			IEnumerable<DataRow> dataRows = constraints.Rows.Cast<DataRow>();
			IEnumerable<string> constraintNameStrings =
				dataRows.Select(row => row["CONSTRAINT_NAME"]?.ToString());
			IEnumerable<string> nonEmptyConstraintNames =
				constraintNameStrings.Where(name => !string.IsNullOrEmpty(name));
			List<string> constraintNames = [.. nonEmptyConstraintNames];

			Assert.That(constraintNames, Contains.Item("SectionsCategories"));
			Assert.That(constraintNames, Contains.Item("SectionsMakers"));

			bool hasPrimaryKeyLikeConstraint = constraintNames.Any(
				name => name.StartsWith("Index_", StringComparison.Ordinal));
			Assert.That(hasPrimaryKeyLikeConstraint, Is.True);

			bool hasCategoriesConstraint =
				constraintNames.Any(name => name.Contains(
					"Categories", StringComparison.Ordinal));
			Assert.That(hasCategoriesConstraint, Is.True);

			foreach (DataRow row in constraints.Rows)
			{
				tableName = row["TABLE_NAME"]?.ToString();
				Assert.That(tableName, Is.EqualTo("Sections"));
			}
		}

		private static string GetTestDatabasePath()
		{
			string fileName = Path.GetTempFileName();

			// A 0 byte sized file is created.  Need to remove it.
			File.Delete(fileName);
			string databasePath = Path.ChangeExtension(fileName, ".db");

			return databasePath;
		}
	}
}
