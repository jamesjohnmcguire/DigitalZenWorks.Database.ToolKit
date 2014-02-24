/////////////////////////////////////////////////////////////////////////////
// $Id: Program.cs 68 2014-02-23 13:57:54Z JamesMc $
//
// Copyright (c) 2006-2014 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Data;
using System.EnterpriseServices;
using System.IO;

using NUnit.Framework;

namespace Zenware.DatabaseLibrary
{
	///////////////////////////////////////////////////////////////////////////
	///// Class <c>DatabaseFixture</c>
	///// <summary>
	///// Base Class for Database Unit Testing
	///// </summary>
	///////////////////////////////////////////////////////////////////////////
	//[TestFixture]
	//[Transaction(TransactionOption.Required)]
	//public class DatabaseFixture : ServicedComponent
	//{
	//    /////////////////////////////////////////////////////////////////////////
	//    /// Method <c>TransactionTearDown</c>
	//    /// <summary>
	//    /// Basic Support for Unit Testing
	//    /// </summary>
	//    /////////////////////////////////////////////////////////////////////////
	//    [TearDown]
	//    public void TransactionTearDown()
	//    {
	//        if (ContextUtil.IsInTransaction)
	//        {
	//            ContextUtil.SetAbort();
	//        }
	//    }
	//}

	/////////////////////////////////////////////////////////////////////////
	/// Class <c>UnitTests</c>
	/// <summary>
	/// Database Unit Testing Class
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	[TestFixture]
	public class TransactionUnitTests
	//public class TransactionUnitTests : DatabaseFixture
	{
		/// <summary>
		/// m_DataLibObject
		/// </summary>
		protected CoreDatabase m_DataLib = null;
		private string dataSource =
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
			@"\data\Projects\Zenware\Dev\Contacts\Src\Product\DatabaseLibraryNET\TestDb.mdb";
		private string dataSourceContacts =
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
			@"\data\admin\Contacts\ContactsX.mdb";
		private string dataSourceBackups =
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
			@"\data\admin\Contacts\backups";
		private string dataSourceBackupsCsv =
			Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) +
			@"\data\admin\Contacts\backups\UnitTests.csv";

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>SetUp</c>
		/// <summary>
		/// function that is called just before each test method is called.
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[SetUp]
		public void SetUp()
		{
			string provider = "Microsoft.Jet.OLEDB.4.0";
			if (Environment.Is64BitOperatingSystem)
			{
				provider = "Microsoft.ACE.OLEDB.12.0";
			}

			m_DataLib = new CoreDatabase(
					"TestDb",
					provider,
					dataSource);

			m_DataLib.Initialize();
			m_DataLib.BeginTransaction();
			//m_sConnectionString = "Initial Catalog=" + p_sCatalog + "; Data Source=" + p_sDataSource + "; user id=" + p_sUserID + "; password=" + p_sPassword;

		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>TearDownSetUp</c>
		/// <summary>
		/// function that is called just after each test method is called.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[TearDown]
		public void TearDown()
		{
			m_DataLib.CommitTransaction();
			m_DataLib.ShutDown();
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>BasicTest</c>
		/// <summary>
		/// Test to see if Unit Testing is working
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[Test]
		public void BasicTest()
		{
			bool AlwaysTrue = true;

			Assert.IsTrue(AlwaysTrue);
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>VerifyTestSourceExists</c>
		/// <summary>
		/// Test to see if test db exisits
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[Test]
		public void VerifyTestSourceExists()
		{
			Assert.IsTrue(File.Exists(dataSource));
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>CanQueryTest</c>
		/// <summary>
		/// Test to see if Unit Testing is working
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[Test]
		public void CanQueryTest()
		{
			bool canQuery = m_DataLib.CanQuery();

			// No exceptions found
			Assert.IsTrue(canQuery);
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>SelectTest</c>
		/// <summary>
		/// Test to see if Unit Testing is working
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		//[Test]
		public void SelectTest()
		{
			bool AlwaysTrue = true;
			DataSet TempDataSet = null;
			string SqlQueryCommand = "SELECT * FROM TestTable";

			CoreDatabase m_oDBLib = null;
			m_oDBLib = new CoreDatabase(
				"TimeTracker",
				"Microsoft.Jet.OLEDB.4.0",
				dataSource);

			m_oDBLib.GetDataSet(SqlQueryCommand, out TempDataSet);

			// No exceptions found
			Assert.IsTrue(AlwaysTrue);
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

			uint NewRowId	= m_DataLib.InsertCommand(SqlQueryCommand);

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

			uint NewRowId = m_DataLib.InsertCommand(SqlQueryCommand);

			SqlQueryCommand = "DELETE FROM TestTable " +
				"WHERE id=" + NewRowId.ToString();

			bool Result = m_DataLib.DeleteCommand(SqlQueryCommand);

			Assert.IsTrue(Result);

			VerifyRowExists(NewRowId, false);
		}


		/////////////////////////////////////////////////////////////////////////
		/// Method <c>Delete</c>
		/// <summary>
		/// Delete Test
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[Test]
		public void ExportToCsv()
		{
			StorageContainers DatabaseHelper = new StorageContainers();

			DatabaseHelper.ExportToCsv(dataSourceContacts, dataSourceBackups);

			Assert.IsTrue((File.Exists(dataSourceBackupsCsv)));
		}

		private void VerifyRowExists(
			uint ExistingRowId,
			bool ShouldExist)
		{
			DataRow TempDataRow = null;
			string SqlQueryCommand = "Select * from TestTable where id=" + ExistingRowId.ToString();

			bool HasData = m_DataLib.GetDataRow(SqlQueryCommand, out TempDataRow);

			Assert.AreEqual(ShouldExist, HasData);
		}
	}	// end class

}	// end namespace
