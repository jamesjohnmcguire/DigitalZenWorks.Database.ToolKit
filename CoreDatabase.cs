using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;
using Common.Logging;
using MySql.Data.MySqlClient;

namespace Zenware.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>CoreDatabase</c>
	/// <summary>
	/// Class for Generic database access independent of the underlying
	/// transport
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class CoreDatabase
	{
		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// DB_OLEDB
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private const uint DB_OLEDB = 1;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// DB_SQLSERVER
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private const uint DB_SQLSERVER = 2;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// DB_ORACLE
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private const uint DB_ORACLE = 3;

		/// <summary>
		/// DB_MYSQL
		/// </summary>
		public const uint DB_MYSQL = 4;

		/// <summary>
		/// m_DatabaseType
		/// </summary>
		private uint m_DatabaseType = 0;

		/// <summary>
		/// The actual connection string used to connect to the database.
		/// </summary>
		private string m_ConnectionString = string.Empty;

		/// <summary>
		/// The name of the client that is using the database.
		/// </summary>
		private string m_ClientName = string.Empty;

		/// <summary>
		/// Database Connection Object
		/// </summary>
		private DbConnection m_Connection = null;

		/// <summary>
		/// Ole Database Connection Object
		/// </summary>
		private OleDbConnection m_OleDbConnection = null;

		private MySqlConnection m_MySqlConnection = null;

		// transactions
		/// <summary>
		///
		/// </summary>
		private DbTransaction m_DatabaseTransaction;

		/// <summary>
		/// Diagnostics object
		/// </summary>
		private ILog log = null;

		/// <summary>
		/// CoreDatabase - Default constructor
		/// </summary>
		public CoreDatabase()
		{
			if ((ConfigurationManager.ConnectionStrings != null) &&
				(ConfigurationManager.ConnectionStrings.Count > 0))
			{
				m_ConnectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

				// OleDbConnection is default
				m_DatabaseType = DB_OLEDB;
				m_ClientName = "DatabaseLib";
				//Initialize(DB_OLEDB, "DatabaseLib");
			}
		}

		/// <summary>
		/// CoreDatabase - Constructor
		/// </summary>
		/// <param name="Provider"></param>
		/// <param name="DataSource"></param>
		public CoreDatabase(
			string Provider,
			string DataSource)
		{
			m_ConnectionString = CreateConnectionString(Provider, DataSource, null);
			m_DatabaseType = DB_OLEDB;
			m_ClientName = "DatabaseLib";
		}

		/// <summary>
		/// CoreDatabase - Constructor
		/// </summary>
		/// <param name="ClientName"></param>
		/// <param name="Provider"></param>
		/// <param name="DataSource"></param>
		public CoreDatabase(
			string ClientName,
			string Provider,
			string DataSource)
		{
			m_ConnectionString = CreateConnectionString(Provider, DataSource, null);
			m_DatabaseType = DB_OLEDB;
			m_ClientName = ClientName;
		}

		/// <summary>
		/// CoreDatabase - Constructor
		/// </summary>
		/// <param name="DatabaseType"></param>
		/// <param name="DataSource"></param>
		/// <param name="Catalog"></param>
		public CoreDatabase(
			uint DatabaseType,
			string DataSource,
			string Catalog)
		{
			m_ConnectionString = CreateConnectionString(null, DataSource, Catalog);
			m_DatabaseType = DatabaseType;
			m_ClientName = "DatabaseLib";
		}

		/// <summary>
		/// CoreDatabase - Constructor
		/// </summary>
		/// <param name="DatabaseType"></param>
		/// <param name="ConnectionString"></param>
		public CoreDatabase(
			uint DatabaseType,
			string ConnectionString)
		{
			m_ConnectionString = ConnectionString;
			m_DatabaseType = DatabaseType;
			m_ClientName = "DatabaseLib";
		}

		/// <summary>
		/// Prepares all necessary class variables by various constructors
		/// </summary>
		public bool Initialize()
		{
			bool ReturnValue = false;
			try
			{
				log = LogManager.GetLogger(this.GetType());

				if (null != m_Connection)
				{
					m_Connection.Close();
				}

				switch (m_DatabaseType)
				{
					case DB_OLEDB:
						{
							m_OleDbConnection = new OleDbConnection(m_ConnectionString);
							m_Connection = m_OleDbConnection;
							break;
						}
					case DB_SQLSERVER:
						{
							m_Connection = new SqlConnection(m_ConnectionString);
							break;
						}
					//case DB_ORACLE:
					//    {
					//        m_Connection = new OracleConnection(m_ConnectionString);
					//        break;
					//    }
					case DB_MYSQL:
						{
							m_MySqlConnection = new MySqlConnection(m_ConnectionString);
							m_Connection = m_MySqlConnection;
							break;
						}
				}

				ReturnValue = true;
			}
			catch (Exception ex)
			{
				log.Debug(CultureInfo.InvariantCulture,  m => m("Initialization Error: {0}", ex.Message));
				throw (ex);
			}
			finally
			{
			}

			return ReturnValue;
		}

		private string CreateConnectionString(
			string Provider,
			string DataSource,
			string Catalog)
		{
			string ConnectionString = null;

			if (null != Provider)
			{
				ConnectionString = "Provider=" + Provider;
			}

			if (null != DataSource)
			{
				ConnectionString += "; Data Source=" + DataSource;
			}

			if (null != Catalog)
			{
				ConnectionString += "; Integrated Security=SSPI; Initial Catalog=" + Catalog;
			}

			return ConnectionString;
		}

		/// <summary>
		/// Establishes connection with the database.
		/// </summary>
		public void EstablishConnection()
		{
			if (null == m_Connection)
			{
				Initialize();
			}
		}

		/// <summary>
		/// Closes the database connection and object
		/// </summary>
		public void Close()
		{
			if (m_Connection != null)
			{
				if (m_Connection.State != ConnectionState.Closed)
					m_Connection.Close();
			}

			System.GC.Collect();
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Shut down the database.
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		public void ShutDown()
		{
			Close();

			if (null != m_Connection)
			{
				m_Connection.Dispose();
				m_Connection = null;
			}

			System.GC.Collect();
		}

		/// <summary>
		/// This opens a connection and begin's the transaction.
		/// </summary>
		public void BeginTransaction()
		{
			EstablishConnection();

			if (null != m_Connection)
			{
				if (m_Connection.State != ConnectionState.Open)
				{
					m_Connection.Open();
				}
				m_DatabaseTransaction = m_Connection.BeginTransaction();
			}
		}

		/// <summary>
		/// This closes the transaction.
		/// </summary>
		private void CloseTransaction()
		{
			if (null != m_DatabaseTransaction)
			{
				m_DatabaseTransaction.Connection.Close();
			}
		}

		/// <summary>
		/// This commits the transaction.
		/// </summary>
		public void CommitTransaction()
		{
			if (null != m_DatabaseTransaction)
			{
				m_DatabaseTransaction.Commit();
			}
		}

		/// <summary>
		/// This rollsback the transaction.
		/// </summary>
		public void RollBackTransaction()
		{
			if (null != m_DatabaseTransaction)
			{
				m_DatabaseTransaction.Rollback();
			}
		}

		private DbCommand GetCommandObject(
			string SqlQuery)
		{
			DbCommand ThisCommand = null;

			try
			{
				EstablishConnection();

				switch (m_DatabaseType)
				{
					case DB_OLEDB:
						{
							ThisCommand = new OleDbCommand();
							break;
						}
					case DB_SQLSERVER:
						{
							ThisCommand = new SqlCommand();
							break;
						}
					//case DB_ORACLE:
					//    {
					//        ThisCommand = new OracleCommand();
					//        break;
					//    }
					case DB_MYSQL:
						{
							ThisCommand = new MySqlCommand();
							break;
						}
				}

				if (m_Connection.State != ConnectionState.Open)
				{
					m_Connection.Open();
				}

				ThisCommand.Transaction = m_DatabaseTransaction;
				ThisCommand.Connection = m_Connection;
				ThisCommand.CommandText = SqlQuery;
				ThisCommand.CommandTimeout = 30;
			}
			catch (Exception ex)
			{
				log.Debug(CultureInfo.InvariantCulture,  m => m("Initialization Error: {0}", ex.Message));
			}
			finally
			{
			}

			return ThisCommand;
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="SqlQuery"></param>
		/// <param name="OutDataSet"></param>
		/// <returns>number of records retrieved</returns>
		/////////////////////////////////////////////////////////////////////////
		public int GetDataSet(
			string SqlQuery,
			out DataSet OutDataSet)
		{
			int RowCount = -1;

			OutDataSet = new DataSet();

			try
			{
				DbCommand ThisCommand = GetCommandObject(SqlQuery);

				DbDataAdapter ThisDataAdapter = null;
				switch (m_DatabaseType)
				{
					case DB_OLEDB:
						{
							ThisDataAdapter = new OleDbDataAdapter();
							break;
						}
					case DB_SQLSERVER:
						{
							ThisDataAdapter = new SqlDataAdapter();
							break;
						}
					//case DB_ORACLE:
					//    {
					//        ThisDataAdapter = new OracleDataAdapter();
					//        break;
					//    }
					case DB_MYSQL:
						{
							ThisDataAdapter = new MySqlDataAdapter();
							break;
						}
				}

				ThisDataAdapter.SelectCommand = ThisCommand;

				RowCount = ThisDataAdapter.Fill(OutDataSet);

				log.Debug(CultureInfo.InvariantCulture,  m => m("OK - getDataSet - Query: {0}", SqlQuery));
			}
			catch (Exception ex)
			{
				log.Debug(CultureInfo.InvariantCulture,  m => m("Initialization Error: {0}", ex.Message));
			}
			finally
			{
				if (null == m_DatabaseTransaction)
				{
					Close();
				}
			}

			return RowCount;
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prepares and executes a Non-Query DB Command
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		public bool ExecuteNonQuery(string SqlQueryCommand)
		{
			bool ReturnCode = false;
			DbCommand ThisCommand = null;

			try
			{
				ThisCommand = GetCommandObject(SqlQueryCommand);
				int RowsEffected = ThisCommand.ExecuteNonQuery();

				if (RowsEffected > 0)
				{
					ReturnCode = true;
				}
			}
			catch (Exception exNonQuery)
			{
				SetExceptionError(
					exNonQuery,
					"ExecuteNonQuery - An exception was encountered while attempting to execute the command. Exception: ",
					SqlQueryCommand);
				throw exNonQuery;
			}
			finally
			{
				if (null == m_DatabaseTransaction)
				{
					Close();
				}

				if (null != ThisCommand)
				{
					ThisCommand.Dispose();
				}
			}
			return ReturnCode;
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prepares and executes a Non-Query DB Command
		/// </summary>
		/////////////////////////////////////////////////////////////////////////
		private uint ExecuteScalar(string SqlQueryCommand)
		{
			uint ReturnCode = 0;
			DbCommand ThisCommand = null;

			try
			{
				if (null != m_Connection)
				{
					if (m_Connection.State != ConnectionState.Open)
					{
						m_Connection.Open();
					}
				}
				ThisCommand = GetCommandObject(SqlQueryCommand);
				Object Result = ThisCommand.ExecuteScalar();

				if (null != Result)
				{
					ReturnCode = Convert.ToUInt32(Result);
				}
			}
			catch (OleDbException Exception)
			{
				SetExceptionError(
					Exception,
					"ExecuteNonQuery - An exception was encountered while attempting to execute the command. Exception: ",
					SqlQueryCommand);
			}
			catch (Exception exNonQuery)
			{
				SetExceptionError(
					exNonQuery,
					"ExecuteNonQuery - An exception was encountered while attempting to execute the command. Exception: ",
					SqlQueryCommand);
			}
			finally
			{
				if (null == m_DatabaseTransaction)
				{
					Close();
				}

				if (null != ThisCommand)
				{
					ThisCommand.Dispose();
				}
			}
			return ReturnCode;
		}

		/// <summary>
		/// Sets an error message of an exception type to the diagnostics object.
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="sIntroMsg"></param>
		/// <param name="sCommand"></param>
		public void SetExceptionError(Exception ex, string sIntroMsg, string sCommand)
		{
			log.Debug(CultureInfo.InvariantCulture,  m => m("Initialization Error: {0}", ex.Message));
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>IsConnected</c>
		/// <summary>
		/// Checks to see if the database is open and connected. Helper function
		/// for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool IsConnected()
		{
			bool Connected = false;

			if (m_Connection.State == ConnectionState.Open)
			{
				Connected = true;
			}

			return Connected;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>CanQuery</c>
		/// <summary>
		/// Checks to see if the database can return a valid query. Helper
		/// function for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool CanQuery()
		{
			bool CanQuery = false;

			DataSet TestDataSet = null;
			int RecordsReturned = -1;

			string SqlQueryCommand = "SELECT @@VERSION";
			RecordsReturned = GetDataSet(SqlQueryCommand, out TestDataSet);

			if (RecordsReturned > 0)
			{
				CanQuery = true;
			}

			return CanQuery;
		}

		/////////////////////////////////////////////////////////////////////////
		/// <summary>
		/// GetDataTable
		/// </summary>
		/// <param name="SqlQueryCommand"></param>
		/// <param name="ReturnedDataTable"> (for returning data)</param>
		/// <returns>number of records retrieved</returns>
		/////////////////////////////////////////////////////////////////////////
		public int GetDataTable(string SqlQueryCommand, out DataTable ReturnedDataTable)
		{
			int RecordsReturned = -1;
			DataSet dtTempSet = null;

			ReturnedDataTable = new DataTable();

			RecordsReturned = GetDataSet(SqlQueryCommand, out dtTempSet);
			if (dtTempSet.Tables.Count > 0)
			{
				ReturnedDataTable = dtTempSet.Tables[0];
			}

			return RecordsReturned;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>GetDataRow</c>
		/// <summary>
		/// Gets a single row of data
		/// </summary>
		/// <param name="SqlCommandQuery"></param>
		/// <param name="DataRowOut"> (for returning data)</param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool GetDataRow(string SqlCommandQuery, out DataRow DataRowOut)
		{
			bool HasData = false;
			int RecordsReturned = -1;
			DataTable TempDataTable = null;

			DataRowOut = null;

			RecordsReturned = GetDataTable(SqlCommandQuery, out TempDataTable);

			if (RecordsReturned > 0)
			{
				DataRowOut = TempDataTable.Rows[0];
				HasData = true;
			}

			return HasData;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>GetDataField</c>
		/// <summary>
		/// Gets a single field from a row of data
		/// </summary>
		/// <param name="SqlCommandQuery"></param>
		/// <returns>the field object</returns>
		/////////////////////////////////////////////////////////////////////////
		public object GetDataField(
			string SqlCommandQuery)
		{
			object DataField = null;
			int RecordsReturned = -1;
			DataTable TempDataTable = null;

			RecordsReturned = GetDataTable(SqlCommandQuery, out TempDataTable);

			if (RecordsReturned > 0)
			{
				DataRow DataRowOut = TempDataTable.Rows[0];
				DataField = DataRowOut.ItemArray[0];
			}

			return DataField;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>UpdateCommand</c>
		/// <summary>
		/// Performs an Sql UPDATE command
		/// </summary>
		/// <param name="SqlQueryCommand"></param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool UpdateCommand(
			string SqlQueryCommand)
		{
			bool ReturnCode = ExecuteNonQuery(SqlQueryCommand);

			return ReturnCode;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>InsertCommand</c>
		/// <summary>
		/// Performs an Sql UPDATE command
		/// </summary>
		/// <param name="SqlQueryCommand"></param>
		/// <returns>object item</returns>
		/////////////////////////////////////////////////////////////////////////
		public uint InsertCommand(string SqlQueryCommand)
		{
			bool FinishTransaction = false;
			if (null == m_DatabaseTransaction)
			{
				BeginTransaction();
				FinishTransaction = true;
			}

			// execute non query
			ExecuteNonQuery(SqlQueryCommand);

			// get id of effected row
			uint ReturnCode = ExecuteScalar("SELECT @@IDENTITY");

			if (true == FinishTransaction)
			{
				CommitTransaction();
			}

			return ReturnCode;
		}

		/////////////////////////////////////////////////////////////////////////
		/// Method <c>DeleteCommand</c>
		/// <summary>
		/// Performs an Sql DELETE command
		/// </summary>
		/// <param name="SqlQueryCommand"></param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////////
		public bool DeleteCommand(string SqlQueryCommand)
		{
			bool ReturnCode = ExecuteNonQuery(SqlQueryCommand);

			return ReturnCode;
		}

		/// <summary>
		/// Temp test method
		/// </summary>
		public void Test()
		{
			int ReturnCode = -1;
			OleDbCommand CommandObject = null;
			OleDbCommand commandObject = null;

			try
			{
				OleDbConnection ThisOleDbConnection = new OleDbConnection(m_ConnectionString);

				string SqlQuery = "INSERT INTO Contacts (Notes) VALUES ('testing')";
				string SqlQuery2 = "SELECT @@IDENTITY";

				CommandObject = new OleDbCommand(SqlQuery2, ThisOleDbConnection);
				commandObject = new OleDbCommand(SqlQuery, ThisOleDbConnection);

				ThisOleDbConnection.Open();

				//CommandObject.CommandTimeout = 30;

				Object Result = commandObject.ExecuteScalar();

				if (null != Result)
				{
					ReturnCode = (int)Result;
				}
				ReturnCode = (int)CommandObject.ExecuteScalar();
				ThisOleDbConnection.Close();
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			finally
			{
				if (CommandObject != null)
				{
					CommandObject.Dispose();
				}
			}
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>GetSchemaTable</c>
		/// <summary>
		/// Get the table schema information for the associated database.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetSchemaTable()
		{
			DataTable Tables = null;

			if (null != m_Connection)
			{
				if (m_Connection.State != ConnectionState.Open)
				{
					m_Connection.Open();
				}
			}

			if (DB_OLEDB == m_DatabaseType)
			{
				Tables = m_OleDbConnection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables,
															new Object[] { null, null, null, "TABLE" });
			}
			else
			{
				//Tables = m_Connection.GetSchema("TABLE");
				Tables = m_Connection.GetSchema();
			}

			return Tables;
		}
	}	// end class
}	// end namespace