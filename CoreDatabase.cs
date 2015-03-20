/////////////////////////////////////////////////////////////////////////////
// $Id: Program.cs 68 2014-02-23 13:57:54Z JamesMc $
//
// Copyright (c) 2006-2015 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
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
		/// databaseType
		/// </summary>
		private uint databaseType = 0;

		/// <summary>
		/// The actual connection string used to connect to the database.
		/// </summary>
		private string connectionString = string.Empty;

		/// <summary>
		/// The name of the client that is using the database.
		/// </summary>
		private string clientName = string.Empty;

		/// <summary>
		/// Database Connection Object
		/// </summary>
		private DbConnection connection = null;

		/// <summary>
		/// Ole Database Connection Object
		/// </summary>
		private OleDbConnection oleDbConnection = null;

		private MySqlConnection mySqlConnection = null;

		// transactions
		/// <summary>
		/// transaction object
		/// </summary>
		private DbTransaction databaseTransaction;

		/// <summary>
		/// Diagnostics object
		/// </summary>
		private ILog log = null;

		private string provider = string.Empty;

		/// <summary>
		/// CoreDatabase - Default constructor
		/// </summary>
		public CoreDatabase()
		{
			if ((ConfigurationManager.ConnectionStrings != null) &&
				(ConfigurationManager.ConnectionStrings.Count > 0))
			{
				connectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;

				// OleDbConnection is default
				databaseType = DB_OLEDB;
				clientName = "DatabaseLib";
				//Initialize(DB_OLEDB, "DatabaseLib");
			}
		}

		/// <summary>
		/// CoreDatabase - Constructor
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="DataSource"></param>
		public CoreDatabase(
			string provider,
			string DataSource)
		{
			this.provider = provider;
			connectionString = CreateConnectionString(provider, DataSource, null);
			databaseType = DB_OLEDB;
			clientName = "DatabaseLib";
		}

		/// <summary>
		/// CoreDatabase - Constructor
		/// </summary>
		/// <param name="ClientName"></param>
		/// <param name="provider"></param>
		/// <param name="DataSource"></param>
		public CoreDatabase(
			string ClientName,
			string provider,
			string DataSource)
		{
			this.provider = provider;
			connectionString = CreateConnectionString(provider, DataSource, null);
			databaseType = DB_OLEDB;
			clientName = ClientName;
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
			connectionString = CreateConnectionString(null, DataSource, Catalog);
			databaseType = DatabaseType;
			clientName = "DatabaseLib";
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
			connectionString = ConnectionString;
			databaseType = DatabaseType;
			clientName = "DatabaseLib";
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

				if (null != connection)
				{
					connection.Close();
				}

				switch (databaseType)
				{
					case DB_OLEDB:
						{
							oleDbConnection = new OleDbConnection(connectionString);
							connection = oleDbConnection;
							break;
						}
					case DB_SQLSERVER:
						{
							connection = new SqlConnection(connectionString);
							break;
						}
					//case DB_ORACLE:
					//    {
					//        connection = new OracleConnection(connectionString);
					//        break;
					//    }
					case DB_MYSQL:
						{
							mySqlConnection = new MySqlConnection(connectionString);
							connection = mySqlConnection;
							break;
						}
				}

				ReturnValue = true;
			}
			catch (Exception ex)
			{
				log.Error(CultureInfo.InvariantCulture, m => m("Initialization Error: {0}", ex.Message));
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
			if (null == connection)
			{
				Initialize();
			}
		}

		/// <summary>
		/// Closes the database connection and object
		/// </summary>
		public void Close()
		{
			if (connection != null)
			{
				if (connection.State != ConnectionState.Closed)
					connection.Close();
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

			if (null != connection)
			{
				connection.Dispose();
				connection = null;
			}

			System.GC.Collect();
		}

		/// <summary>
		/// This opens a connection and begin's the transaction.
		/// </summary>
		public void BeginTransaction()
		{
			try
			{
				EstablishConnection();

				if (null != connection)
				{
					if (connection.State != ConnectionState.Open)
					{
						connection.Open();
					}
					databaseTransaction = connection.BeginTransaction();
				}
			}
			catch (Exception ex)
			{
				log.Error(CultureInfo.InvariantCulture,
					m => m("BeginTransaction Error: {0}", ex.Message));
			}
		}

		/// <summary>
		/// This closes the transaction.
		/// </summary>
		private void CloseTransaction()
		{
			if (null != databaseTransaction)
			{
				databaseTransaction.Connection.Close();
			}
		}

		/// <summary>
		/// This commits the transaction.
		/// </summary>
		public void CommitTransaction()
		{
			if (null != databaseTransaction)
			{
				databaseTransaction.Commit();
			}
		}

		/// <summary>
		/// This rollsback the transaction.
		/// </summary>
		public void RollBackTransaction()
		{
			if (null != databaseTransaction)
			{
				databaseTransaction.Rollback();
			}
		}

		private DbCommand GetCommandObject(
			string SqlQuery)
		{
			DbCommand ThisCommand = null;

			try
			{
				EstablishConnection();

				switch (databaseType)
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

				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}

				ThisCommand.Transaction = databaseTransaction;
				ThisCommand.Connection = connection;
				ThisCommand.CommandText = SqlQuery;
				ThisCommand.CommandTimeout = 30;
			}
			catch (Exception ex)
			{
				log.Error(CultureInfo.InvariantCulture, m => m("Initialization Error: {0}", ex.Message));
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
				switch (databaseType)
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

				log.Info(CultureInfo.InvariantCulture, m => m("OK - getDataSet - Query: {0}", SqlQuery));
			}
			catch (Exception ex)
			{
				log.Error(CultureInfo.InvariantCulture, m => m("Initialization Error: {0}", ex.Message));
			}
			finally
			{
				if (null == databaseTransaction)
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
				if (null == databaseTransaction)
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
				if (null != connection)
				{
					if (connection.State != ConnectionState.Open)
					{
						connection.Open();
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
				if (null == databaseTransaction)
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
			log.Error(CultureInfo.InvariantCulture, m => m("Initialization Error: {0}", ex.Message));
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

			if (connection.State == ConnectionState.Open)
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

			if (databaseType == DB_OLEDB)
			{
				SqlQueryCommand = "SELECT VERSION";
			}

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
			if (null == databaseTransaction)
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
				OleDbConnection ThisOleDbConnection = new OleDbConnection(connectionString);

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

			if (null != connection)
			{
				if (connection.State != ConnectionState.Open)
				{
					connection.Open();
				}
			}

			if (DB_OLEDB == databaseType)
			{
				Tables = oleDbConnection.GetOleDbSchemaTable(System.Data.OleDb.OleDbSchemaGuid.Tables,
															new Object[] { null, null, null, "TABLE" });
			}
			else
			{
				//Tables = connection.GetSchema("TABLE");
				Tables = connection.GetSchema();
			}

			return Tables;
		}
	}	// end class
}	// end namespace