/////////////////////////////////////////////////////////////////////////////
// $Id$
//
// Copyright © 2006 - 2016 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using NUnit.Framework;
using System;
using System.Data;
using System.Data.OleDb;
using System.EnterpriseServices;
using System.IO;

namespace DigitalZenWorks.Common.DatabaseLibrary
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

			connectionString = string.Format("provider={0}; Data Source={1}",
				provider, dataSource);
			OleDbConnection oleDbConnection =
				new OleDbConnection(connectionString);

			oleDbConnection.Open();
			oleDbConnection.Close();

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
			DataSet TempDataSet = null;
			string SqlQueryCommand = "SELECT * FROM TestTable";

			int count = database.GetDataSet(SqlQueryCommand, out TempDataSet);

			// No exceptions found
			Assert.GreaterOrEqual(count, 0);
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

			uint NewRowId	= database.Insert(SqlQueryCommand);

			VerifyRowExists(NewRowId, true);
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
			string Description = "Unit Test - Time: " + DateTime.Now;
			string SqlQueryCommand = "INSERT INTO TestTable " +
							@"(Description) VALUES " +
							@"('" + Description + "')";

			uint NewRowId = database.Insert(SqlQueryCommand);

			SqlQueryCommand = "DELETE FROM TestTable " +
				"WHERE id=" + NewRowId;

			bool Result = database.Delete(SqlQueryCommand);

			Assert.IsTrue(Result);

			VerifyRowExists(NewRowId, false);
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

		private void VerifyRowExists(
			uint ExistingRowId,
			bool ShouldExist)
		{
			DataRow TempDataRow = null;
			string SqlQueryCommand = "Select * from TestTable where id=" + ExistingRowId;

			bool HasData = database.GetDataRow(SqlQueryCommand, out TempDataRow);

			Assert.AreEqual(ShouldExist, HasData);
		}
	}	// end class

}	// end namespace
