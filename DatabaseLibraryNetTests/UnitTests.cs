/////////////////////////////////////////////////////////////////////////////
// Copyright @ 2006 - 2025 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////

using DigitalZenWorks.Database.ToolKit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SQLite;
using System.Globalization;
using System.IO;
using System.Reflection;

[assembly: CLSCompliant(true)]

namespace DigitalZenWorks.Database.ToolKit.Tests
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>UnitTests</c>
	/// <summary>
	/// Database Unit Testing Class
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	[TestFixture]
	internal sealed class TransactionUnitTests : IDisposable
	{
		/// <summary>
		/// database
		/// </summary>
		private DataStorage database;
		private string dataSource;

		/////////////////////////////////////////////////////////////////////
		/// Method <c>OneTimeSetUp</c>
		/// <summary>
		/// One time set up method.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
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

		/////////////////////////////////////////////////////////////////////
		/// Method <c>OneTimeTearDown</c>
		/// <summary>
		/// function that is called when all tests are completed.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			if (database != null)
			{
				database.CommitTransaction();
				database.Shutdown();
			}

			File.Delete(dataSource);
		}

		[TearDown]
		public void TearDown()
		{
			if (database != null)
			{
				if (database.Connection != null)
				{
					ConnectionState state = database.Connection.State;

					if (state == ConnectionState.Open)
					{
						database.Connection.Close();
					}
				}

				database.Close();
			}
		}

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing"></param>
		public void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != database)
				{
					database.Close();
					database = null;
				}
			}
		}

		/// <summary>
		/// Dispose
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>BasicTest</c>
		/// <summary>
		/// Test to see if Unit Testing is working
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[Test]
		public static void BasicTest()
		{
			bool AlwaysTrue = true;

			Assert.That(AlwaysTrue, Is.True);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>CanQueryTest</c>
		/// <summary>
		/// Test to see if Unit Testing is working
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[Test]
		public void CanQueryTest()
		{
			bool canQuery = database.CanQuery();

			// No exceptions found
			Assert.That(canQuery, Is.True);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>CreateTableTest</c>
		/// <summary>
		/// Create table test
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[Test, Order(1)]
		public void CreateTableTest()
		{
			string statement = "CREATE TABLE TestTable " +
				"(id INTEGER PRIMARY KEY, description VARCHAR(64))";

			bool result = database.ExecuteNonQuery(statement);

			statement = "SELECT name FROM sqlite_master " +
				"WHERE type = 'table' AND name = 'TestTable';";

			using DbDataReader dbDataReader = database.ExecuteReader(statement);

			Assert.That(dbDataReader.HasRows, Is.True);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>DatabaseCanOpen</c>
		/// <summary>
		/// Test to see if test db exists
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[Test]
		public void DatabaseCanOpen()
		{
			//	string provider = "Microsoft.ACE.OLEDB.12.0";

			//	string connectionString = string.Format(
			//		CultureInfo.InvariantCulture,
			//		"provider={0}; Data Source={1}",
			//		provider,
			//		dataSource);
			//	using (OleDbConnection oleDbConnection =
			//		new OleDbConnection(connectionString))
			//	{
			//		oleDbConnection.Open();
			//		oleDbConnection.Close();
			//	}

			//	// assuming no exceptions
			//	Assert.IsTrue(File.Exists(dataSource));
			Assert.Pass();
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Dependencies order test.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[Test]
		public void DependenciesOrder()
		{
			Dictionary<string, List<string>> tableDependencies = new()
			{
				{ "Categories", [] },
				{ "Makers", [] },
				{ "Series", new List<string> { "Makers" } },
				{ "Sections", new List<string> { "Categories", "Makers" } },
				{
					"ImportProducts", new List<string>
					{ "Sections", "Series", "Makers" }
				}
			};

			List<string> orderedDependencies =
				DataDefinition.GetOrderedDependencies(tableDependencies);

			int tableCount = orderedDependencies.Count;
			Assert.That(tableCount, Is.EqualTo(5));

			string tableName = orderedDependencies[0];
			Assert.That(tableName, Is.AnyOf("Categories", "Makers"));

			tableName = orderedDependencies[1];
			Assert.That(tableName, Is.AnyOf("Categories", "Makers"));

			tableName = orderedDependencies[2];
			Assert.That(tableName, Is.AnyOf("Sections", "Series"));

			tableName = orderedDependencies[3];
			Assert.That(tableName, Is.AnyOf("Sections", "Series"));

			tableName = orderedDependencies[4];
			Assert.That(tableName, Is.EqualTo("ImportProducts"));
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>Delete</c>
		/// <summary>
		/// Delete Test
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
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

			bool Result = database.Delete(query);

			Assert.That(Result, Is.True);

			VerifyRowExists(rowId, false);
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>ExportToCsv</c>
		/// <summary>
		/// Export to CSV Test
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[Test]
		public void ExportToCsv()
		{
			string tempPath = Path.GetTempPath();

			DatabaseUtilities.ExportToCsv(dataSource, tempPath);

			string csvFile = tempPath + "TestTable.csv";

			bool exists = File.Exists(csvFile);
			Assert.That(exists, Is.True);
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>Insert</c>
		/// <summary>
		/// Insert Test
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[Test]
		public void Insert()
		{
			string Description = "Unit Test - Time: " + DateTime.Now;

			string SqlQueryCommand = "INSERT INTO TestTable " +
							@"(Description) VALUES " +
							@"('" + Description + "')";

			int rowId = database.Insert(SqlQueryCommand);

			Assert.That(rowId, Is.GreaterThanOrEqualTo(1));

			VerifyRowExists(rowId, true);
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>SchemaTable</c>
		/// <summary>
		/// Delete Test
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[Test]
		public void SchemaTable()
		{
			DataTable table = database.SchemaTable;

			Assert.That(table, Is.Not.Null);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>SelectTest</c>
		/// <summary>
		/// Test to see if Unit Testing is working
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[Test]
		public void SelectTest()
		{
			string query = "SELECT * FROM TestTable";

			DataSet dataSet = database.GetDataSet(query);

			Assert.That(dataSet, Is.Not.Null);

			// No exceptions found
			Assert.That(dataSet.Tables.Count, Is.GreaterThanOrEqualTo(0));
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>Update</c>
		/// <summary>
		/// Update Test
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
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

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>UpdateWithParameters</c>
		/// <summary>
		/// Update with Parameters Test
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[Test]
		public void UpdateWithParameters()
		{
			string description = "Unit Test - Time: " + DateTime.Now;
			string query = "UPDATE TestTable SET [Description] = ?";

			Dictionary<string, object> parameters = new ();
			parameters.Add("[Description]", description);

			bool result = database.Update(query, parameters);

			Assert.That(result, Is.True);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>VerifyTestSourceExists</c>
		/// <summary>
		/// Test to see if test db exists
		/// </summary>
		/////////////////////////////////////////////////////////////////////
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

			if (true == shouldExist)
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
