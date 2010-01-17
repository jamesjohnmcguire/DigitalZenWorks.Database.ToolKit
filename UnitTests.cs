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

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>SetUp</c>
		/// <summary>
		/// function that is called just before each test method is called.
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		[SetUp]
		public void SetUp()
		{
			m_DataLib = new CoreDatabase(
					"TestDb",
					"Microsoft.Jet.OLEDB.4.0",
					@"C:\data\tech\Projects\Zenware\Dev\Common\Src\DatabaseLibraryNet\TestDb.mdb");

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
			Assert.IsTrue(File.Exists(@"C:\data\tech\Projects\Zenware\Dev\Common\Src\DatabaseLibraryNet\TestDb.mdb"));
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
			bool AlwaysTrue = true;

			m_DataLib.CanQuery();

			// No exceptions found
			Assert.IsTrue(AlwaysTrue);
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
				//@"C:\data\tech\projects\Zenware\projects\TimeTracker\TimeTracker.mdb");
				@"C:\data\tech\Projects\Zenware\Common\src\DBLib\TestDb.mdb");

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

			DatabaseHelper.ExportToCsv(@"c:\data\admin\contacts\ContactsX.mdb", @"c:\data\admin\contacts\backups");

			Assert.IsTrue((File.Exists(@"c:\data\admin\contacts\backups\contacts.csv")));
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
