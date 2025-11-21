// <copyright file="UnitTests.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>

[assembly: System.CLSCompliant(true)]

namespace DigitalZenWorks.Database.ToolKit.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Data.Common;
	using System.Data.SQLite;
	using System.Globalization;
	using System.IO;
	using NUnit.Framework;

	/// <summary>
	/// Database Unit Testing Class.
	/// </summary>
	[TestFixture]
	internal sealed class TransactionUnitTests : IDisposable
	{
		/// <summary>
		/// database storage object.
		/// </summary>
		private DataStorage database;
		private string dataSource;

		/// <summary>
		/// Test to see if Unit Testing is working.
		/// </summary>
		[Test]
		public static void BasicTest()
		{
			bool alwaysTrue = true;

			Assert.That(alwaysTrue, Is.True);
		}

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
		/// Dispose method.
		/// </summary>
		/// <param name="disposing">True to release both managed and unmanaged
		/// resources; false to release only unmanaged resources.</param>
		public void Dispose(bool disposing)
		{
			if (disposing)
			{
				database?.Close();
				database = null;
			}
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Test to see if Unit Testing is working.
		/// </summary>
		[Test]
		public void CanQueryTest()
		{
			bool canQuery = database.CanQuery();

			// No exceptions found
			Assert.That(canQuery, Is.True);
		}

		/// <summary>
		/// Create table test.
		/// </summary>
		[Test]
		[Order(1)]
		public void CreateTableTest()
		{
			string statement = "CREATE TABLE TestTable " +
				"(id INTEGER PRIMARY KEY, description VARCHAR(64))";

			bool result = database.ExecuteNonQuery(statement);
			Assert.That(result, Is.True);

			statement = "SELECT name FROM sqlite_master " +
				"WHERE type = 'table' AND name = 'TestTable';";

			using DbDataReader dbDataReader = database.ExecuteReader(statement);

			Assert.That(dbDataReader.HasRows, Is.True);
		}

		/// <summary>
		/// Dependencies order test.
		/// </summary>
		[Test]
		public void DependenciesOrder()
		{
			Dictionary<string, Collection<string>> tableDependencies = new()
			{
				{ "Addresses", [] },
				{ "Categories", new Collection<string> { "Categories" } },
				{ "Contacts", new Collection<string> { "Addresses" } },
				{ "Makers", [] },
				{ "Series", new Collection<string> { "Makers" } },
				{
					"Sections", new Collection<string>
					{ "Categories", "Makers" }
				},
				{
					"Products", new Collection<string>
					{ "Sections", "Series", "Makers" }
				}
			};

			Collection<string> orderedDependencies =
				DataDefinition.GetOrderedDependencies(tableDependencies);

			int tableCount = orderedDependencies.Count;
			Assert.That(tableCount, Is.EqualTo(7));

			string tableName = orderedDependencies[0];
			Assert.That(
				tableName, Is.AnyOf("Addresses", "Categories", "Makers"));

			tableName = orderedDependencies[1];
			Assert.That(
				tableName, Is.AnyOf(
					"Addresses", "Categories", "Contacts", "Makers"));

			tableName = orderedDependencies[2];
			Assert.That(
				tableName, Is.AnyOf(
					"Addresses", "Categories", "Contacts", "Makers"));

			tableName = orderedDependencies[3];
			Assert.That(
				tableName, Is.AnyOf(
					"Addresses", "Categories", "Contacts", "Makers"));

			tableName = orderedDependencies[6];
			Assert.That(tableName, Is.EqualTo("Products"));
		}

		/// <summary>
		/// Delete Test.
		/// </summary>
		[Test]
		public void Delete()
		{
			string description = "Unit Test - Time: " + DateTime.Now;
			string query = string.Format(
				CultureInfo.InvariantCulture,
				"INSERT INTO TestTable (Description) VALUES ('{0}')",
				description);

			int rowId = database.Insert(query);

			query = "DELETE FROM TestTable WHERE id=" + rowId;

			bool result = database.Delete(query);

			Assert.That(result, Is.True);

			VerifyRowExists(rowId, false);
		}

		/// <summary>
		/// Export to CSV Test.
		/// </summary>
		[Test]
		public void ExportToCsv()
		{
			string tempPath = Path.GetTempPath();

			DatabaseUtilities.ExportToCsv(dataSource, tempPath);

			string csvFile = tempPath + "TestTable.csv";

			bool exists = File.Exists(csvFile);
			Assert.That(exists, Is.True);
		}

		/// <summary>
		/// Insert Test.
		/// </summary>
		[Test]
		public void Insert()
		{
			string description = "Unit Test - Time: " + DateTime.Now;

			string sqlQueryCommand = "INSERT INTO TestTable " +
							@"(Description) VALUES " +
							@"('" + description + "')";

			int rowId = database.Insert(sqlQueryCommand);

			Assert.That(rowId, Is.GreaterThanOrEqualTo(1));

			VerifyRowExists(rowId, true);
		}

		/// <summary>
		/// Delete Test.
		/// </summary>
		[Test]
		public void SchemaTable()
		{
			DataTable table = database.SchemaTable;

			Assert.That(table, Is.Not.Null);
		}

		/// <summary>
		/// Test to see if Unit Testing is working.
		/// </summary>
		[Test]
		public void SelectTest()
		{
			string query = "SELECT * FROM TestTable";

			DataSet dataSet = database.GetDataSet(query);

			Assert.That(dataSet, Is.Not.Null);
			Assert.That(dataSet.Tables, Has.Count.GreaterThanOrEqualTo(0));
		}

		/// <summary>
		/// Update Test.
		/// </summary>
		[Test]
		public void Update()
		{
			string description = "Unit Test - Time: " + DateTime.Now;
			string query = string.Format(
				CultureInfo.InvariantCulture,
				"UPDATE TestTable SET [Description] = '{0}'",
				description);

			bool result = database.Update(query);

			Assert.That(result, Is.True);
		}

		/// <summary>
		/// Update with Parameters Test.
		/// </summary>
		[Test]
		public void UpdateWithParameters()
		{
			string description = "Unit Test - Time: " + DateTime.Now;
			string query = "UPDATE TestTable SET [Description] = ?";

			Dictionary<string, object> parameters = [];
			parameters.Add("[Description]", description);

			bool result = database.Update(query, parameters);

			Assert.That(result, Is.True);
		}

		/// <summary>
		/// Test to see if test db exists.
		/// </summary>
		[Test]
		public void VerifyTestSourceExists()
		{
			Assert.That(File.Exists(dataSource), Is.True);
		}

		private static string GetTestDatabasePath()
		{
			string fileName = Path.GetTempFileName();

			// A 0 byte sized file is created.  Need to remove it.
			File.Delete(fileName);
			string databasePath = Path.ChangeExtension(fileName, ".db");

			return databasePath;
		}

		private void VerifyRowExists(int existingRowId, bool shouldExist)
		{
			string sql = "Select * from TestTable where Id=" + existingRowId;

			DataRow tempDataRow = database.GetDataRow(sql);

			if (shouldExist == true)
			{
				Assert.That(tempDataRow, Is.Not.Null);
			}
			else
			{
				Assert.That(tempDataRow, Is.Null);
			}
		}
	}
}
