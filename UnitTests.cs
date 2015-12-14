/////////////////////////////////////////////////////////////////////////////
// $Id$
//
// Copyright (c) 2006-2015 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Data;
using System.EnterpriseServices;
using System.IO;

using NUnit.Framework;

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
	public class TransactionUnitTests
	{
		/// <summary>
		/// database
		/// </summary>
		private DataStorage database = null;
		private string dataSource = AppDomain.CurrentDomain.BaseDirectory +
			"TestDb.mdb";
		private string dataSourceBackupsCsv = 
			AppDomain.CurrentDomain.BaseDirectory + @"\TestTable.csv";

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Setup</c>
		/// <summary>
		/// function that is called just before each test method is called.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[SetUp]
		public void Setup()
		{
			string provider = "Microsoft.Jet.OLEDB.4.0";
			if (Environment.Is64BitProcess)
			{
				provider = "Microsoft.ACE.OLEDB.12.0";
			}

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
			string provider = "Microsoft.Jet.OLEDB.4.0";
			if (Environment.Is64BitOperatingSystem)
			{
				provider = "Microsoft.ACE.OLEDB.12.0";
			}

			DataSet TempDataSet = null;
			string SqlQueryCommand = "SELECT * FROM TestTable";

			DataStorage dataStorage = new DataStorage(provider, dataSource);

			int count =
				dataStorage.GetDataSet(SqlQueryCommand, out TempDataSet);

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
