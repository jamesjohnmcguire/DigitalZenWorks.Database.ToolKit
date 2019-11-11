/////////////////////////////////////////////////////////////////////////////
// Copyright � 2006 - 2019 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////

using DigitalZenWorks.Common.DatabaseLibrary;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.EnterpriseServices;
using System.Globalization;
using System.IO;

namespace DigitalZenWorks.Common.DatabaseLibrary.Tests
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>UnitTests</c>
	/// <summary>
	/// Database Unit Testing Class
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	[TestFixture]
	[Transaction(TransactionOption.Required)]
	public class TransactionUnitTests: IDisposable
	{
		/// <summary>
		/// database
		/// </summary>
		private DataStorage database = null;
		private string dataSource = AppDomain.CurrentDomain.BaseDirectory +
			"TestDb.mdb";
		private string dataSourceBackupsCsv = 
			AppDomain.CurrentDomain.BaseDirectory + @"\TestTable.csv";
		private string provider = "Microsoft.ACE.OLEDB.12.0";

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Setup</c>
		/// <summary>
		/// function that is called just before each test method is called.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[SetUp]
		public void Setup()
		{
			database = new DataStorage(provider, dataSource);

			database.BeginTransaction();
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Teardown</c>
		/// <summary>
		/// function that is called just after each test method is called.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[TearDown]
		public void Teardown()
		{
			if (ContextUtil.IsInTransaction)
			{
				ContextUtil.SetAbort();
			}

			database.CommitTransaction();
			database.Shutdown();
		}

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
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

			Assert.IsTrue(AlwaysTrue);
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
			string connectionString = null;

			connectionString = string.Format(
				CultureInfo.InvariantCulture,
				"provider={0}; Data Source={1}",
				provider,
				dataSource);
			using (OleDbConnection oleDbConnection =
				new OleDbConnection(connectionString))
			{
				oleDbConnection.Open();
				//oleDbConnection.Close();
			}

			// assuming no exceptions
			Assert.IsTrue(File.Exists(dataSource));
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
			Assert.IsTrue(File.Exists(dataSource));
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
			Assert.IsTrue(canQuery);
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

			// No exceptions found
			Assert.GreaterOrEqual(dataSet.Tables.Count, 0);
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

			VerifyRowExists(rowId, true);
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

			Assert.IsTrue(Result);

			VerifyRowExists(rowId, false);
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>ExportToCsv</c>
		/// <summary>
		/// Delete Test
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[Test]
		public void ExportToCsv()
		{
			DatabaseUtilities.ExportToCsv(dataSource,
				AppDomain.CurrentDomain.BaseDirectory);

			Assert.IsTrue((File.Exists(dataSourceBackupsCsv)));
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

			Assert.NotNull(table);
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

			Assert.True(result);
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

			IDictionary<string, object> parameters =
				new Dictionary<string, object>();
			parameters.Add("[Description]", description);

			bool result = database.Update(query, parameters);

			Assert.True(result);
		}

		private void VerifyRowExists(int existingRowId, bool shouldExist)
		{
			DataRow tempDataRow = null;
			string sql = "Select * from TestTable where Id=" + existingRowId;

			tempDataRow = database.GetDataRow(sql);

			if (true == shouldExist)
			{
				Assert.NotNull(tempDataRow);
			}
			else
			{
				Assert.IsNull(tempDataRow);
			}
		}
	}	// end class

}	// end namespace