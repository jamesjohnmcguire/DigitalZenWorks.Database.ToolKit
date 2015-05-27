/////////////////////////////////////////////////////////////////////////////
// $Id: $
//
// Copyright (c) 2006-2015 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using Common.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Globalization;

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
		#region private variables
		/// <summary>
		/// databaseType
		/// </summary>
		private DatabaseTypes databaseType;

		/// <summary>
		/// The actual connection string used to connect to the database.
		/// </summary>
		private string connectionString = string.Empty;

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
		private static readonly ILog log = LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string provider = string.Empty;
		#endregion private variables

		#region constructors
		/// <summary>
		/// CoreDatabase - Default constructor
		/// </summary>
		public CoreDatabase()
		{
			if ((ConfigurationManager.ConnectionStrings != null) &&
				(ConfigurationManager.ConnectionStrings.Count > 0))
			{
				connectionString =
					ConfigurationManager.ConnectionStrings[0].ConnectionString;

				// OleDbConnection is default
				databaseType = DatabaseTypes.OleDb;
			}
		}

		/// <summary>
		/// CoreDatabase - Constructor
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="dataSource"></param>
		public CoreDatabase(string provider, string dataSource)
		{
			this.provider = provider;
			connectionString = CreateConnectionString(dataSource, null);
			databaseType = DatabaseTypes.OleDb;
		}

		/// <summary>
		/// CoreDatabase - Constructor
		/// </summary>
		/// <param name="databaseType"></param>
		/// <param name="dataSource"></param>
		/// <param name="catalog"></param>
		public CoreDatabase(DatabaseTypes databaseType, string dataSource,
			string catalog)
		{
			connectionString = CreateConnectionString(dataSource, catalog);
			this.databaseType = databaseType;
		}

		/// <summary>
		/// CoreDatabase - Constructor
		/// </summary>
		/// <param name="databaseType"></param>
		/// <param name="ConnectionString"></param>
		public CoreDatabase(DatabaseTypes databaseType, string ConnectionString)
		{
			connectionString = ConnectionString;
			this.databaseType = databaseType;
		}
		#endregion constructors

		#region startup and shutdown
		/// <summary>
		/// Closes the database connection and object
		/// </summary>
		public void Close()
		{
			if (null != connection)
			{
				connection.Dispose();
				connection = null;
			}
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Shut down the database.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public void Shutdown()
		{
			Close();

			// not unless we find it's really needed
			//System.GC.Collect();
		}
		#endregion startup and shutdown

		#region transactions
		/// <summary>
		/// This opens a connection and begins the transaction.
		/// </summary>
		public void BeginTransaction()
		{
			try
			{
				bool returnCode = Initialize();

				if (true == returnCode)
				{
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
		public void CloseTransaction()
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
		/// This rolls back the transaction.
		/// </summary>
		public void RollbackTransaction()
		{
			if (null != databaseTransaction)
			{
				databaseTransaction.Rollback();
			}
		}
		#endregion transactions

		#region methods
		/////////////////////////////////////////////////////////////////////
		/// Method <c>CanQuery</c>
		/// <summary>
		/// Checks to see if the database can return a valid query. Helper
		/// function for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise</returns>
		/////////////////////////////////////////////////////////////////////
		public bool CanQuery()
		{
			bool canQuery = false;

			DataTable Tables = GetSchemaTable();

			if (Tables.Rows.Count > 0)
			{
				canQuery = true;
			}

			return canQuery;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Delete</c>
		/// <summary>
		/// Performs an Sql DELETE command
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////
		public bool Delete(string sql)
		{
			bool returnCode = ExecuteNonQuery(sql);

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prepares and executes a Non-Query DB Command
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public bool ExecuteNonQuery(string sql)
		{
			bool returnCode = false;
			DbCommand command = null;

			try
			{
				command = GetCommandObject(sql);

				if (null != command)
				{
					int rowsEffected = command.ExecuteNonQuery();

					if (rowsEffected > 0)
					{
						returnCode = true;
					}
				}
			}
			catch (Exception exception)
			{
				log.Error(CultureInfo.InvariantCulture,
					m => m("Exception: {0} Command: {1}", exception.Message,
					sql));
				throw exception;
			}
			finally
			{
				if (null == databaseTransaction)
				{
					Close();
				}

				if (null != command)
				{
					command.Dispose();
				}
			}
			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>GetDataField</c>
		/// <summary>
		/// Gets a single field from a row of data
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>the field object</returns>
		/////////////////////////////////////////////////////////////////////
		public object GetDataField(string sql)
		{
			object datafield = null;
			int recordsReturned = -1;
			DataTable TempDataTable = null;

			recordsReturned = GetDataTable(sql, out TempDataTable);

			if (recordsReturned > 0)
			{
				DataRow DataRowOut = TempDataTable.Rows[0];
				datafield = DataRowOut.ItemArray[0];
			}

			return datafield;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>GetDataRow</c>
		/// <summary>
		/// Gets a single row of data
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="dataRowOut"> (for returning data)</param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////
		public bool GetDataRow(string sql, out DataRow dataRowOut)
		{
			bool hasData = false;
			int recordsReturned = -1;
			DataTable dataTable = null;

			dataRowOut = null;

			recordsReturned = GetDataTable(sql, out dataTable);

			if (recordsReturned > 0)
			{
				dataRowOut = dataTable.Rows[0];
				hasData = true;
			}

			return hasData;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="dataset"></param>
		/// <returns>number of records retrieved</returns>
		/////////////////////////////////////////////////////////////////////
		public int GetDataSet(string sql, out DataSet dataset)
		{
			int rowCount = -1;

			dataset = new DataSet();

			try
			{
				DbCommand command = GetCommandObject(sql);

				if (null != command)
				{
					DbDataAdapter dataAdapter = null;
					switch (databaseType)
					{
						case DatabaseTypes.OleDb:
						{
							dataAdapter = new OleDbDataAdapter();
							break;
						}
						case DatabaseTypes.SqlServer:
						{
							dataAdapter = new SqlDataAdapter();
							break;
						}
						case DatabaseTypes.MySql:
						{
							dataAdapter = new MySqlDataAdapter();
							break;
						}
					}

					dataAdapter.SelectCommand = command;

					rowCount = dataAdapter.Fill(dataset);

					log.Info(CultureInfo.InvariantCulture,
						m => m("OK - getDataSet - Query: {0}", sql));
				}
			}
			catch (Exception ex)
			{
				log.Error(CultureInfo.InvariantCulture,
					m => m("Initialization Error: {0}", ex.Message));
			}
			finally
			{
				if (null == databaseTransaction)
				{
					Close();
				}
			}

			return rowCount;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// GetDataTable
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="dataTable"> (for returning data)</param>
		/// <returns>number of records retrieved</returns>
		/////////////////////////////////////////////////////////////////////
		public int GetDataTable(string sql, out DataTable dataTable)
		{
			int recordsReturned = -1;
			DataSet dataSet = null;

			dataTable = new DataTable();

			recordsReturned = GetDataSet(sql, out dataSet);
			if (dataSet.Tables.Count > 0)
			{
				dataTable = dataSet.Tables[0];
			}

			return recordsReturned;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Insert</c>
		/// <summary>
		/// Performs an Sql UPDATE command
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>object item</returns>
		/////////////////////////////////////////////////////////////////////
		public uint Insert(string sql)
		{
			bool finishTransaction = false;
			if (null == databaseTransaction)
			{
				BeginTransaction();
				finishTransaction = true;
			}

			// execute non query
			ExecuteNonQuery(sql);

			// get id of effected row
			uint returnCode = ExecuteScalar("SELECT @@IDENTITY");

			if (true == finishTransaction)
			{
				CommitTransaction();
			}

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>IsConnected</c>
		/// <summary>
		/// Checks to see if the database is open and connected. Helper function
		/// for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise</returns>
		/////////////////////////////////////////////////////////////////////
		public bool IsConnected()
		{
			bool connected = false;

			if ((null != connection) &&
				(connection.State == ConnectionState.Open))
			{
				connected = true;
			}

			return connected;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Update</c>
		/// <summary>
		/// Performs an Sql UPDATE command
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////
		public bool Update(string sql)
		{
			bool returnCode = ExecuteNonQuery(sql);

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Temp test method
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public void Test()
		{
			int returnCode = -1;
			OleDbCommand commandObject1 = null;
			OleDbCommand commandObject2 = null;

			try
			{
				OleDbConnection connection =
					new OleDbConnection(connectionString);

				string sql =
					"INSERT INTO Contacts (Notes) VALUES ('testing')";
				string Sql2 = "SELECT @@IDENTITY";

				commandObject1 = new OleDbCommand(sql, connection);
				commandObject2 = new OleDbCommand(Sql2, connection);

				connection.Open();

				//CommandObject.CommandTimeout = 30;

				Object result = commandObject1.ExecuteScalar();

				if (null != result)
				{
					returnCode = (int)result;
				}

				returnCode = (int)commandObject2.ExecuteScalar();
			}
			catch (Exception ex)
			{
				throw (ex);
			}
			finally
			{
				Close();

				if (commandObject1 != null)
				{
					commandObject1.Dispose();
					commandObject1 = null;
				}

				if (commandObject2 != null)
				{
					commandObject2.Dispose();
					commandObject2 = null;
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
			DataTable tables = null;

			bool returnCode = Initialize();

			if (true == returnCode)
			{
				if (DatabaseTypes.OleDb == databaseType)
				{
					tables = oleDbConnection.GetOleDbSchemaTable(
						System.Data.OleDb.OleDbSchemaGuid.Tables,
						new Object[] { null, null, null, "TABLE" });
				}
				else
				{
					//tables = connection.GetSchema("TABLE");
					tables = connection.GetSchema();
				}
			}

			return tables;
		}
		#endregion methods

		private string CreateConnectionString(string dataSource,
			string catalog)
		{
			string connectionString = null;

			if (null != provider)
			{
				connectionString = "provider=" + provider;
			}

			if (null != dataSource)
			{
				connectionString += "; Data Source=" + dataSource;
			}

			if (null != catalog)
			{
				connectionString +=
					"; Integrated Security=SSPI; Initial catalog=" + catalog;
			}

			return connectionString;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The first column of the first row in the result set,
		/// or a null reference if the result set is empt
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private uint ExecuteScalar(string sql)
		{
			uint returnCode = 0;
			DbCommand command = null;

			try
			{
				command = GetCommandObject(sql);

				if (null != command)
				{
					Object Result = command.ExecuteScalar();

					if (null != Result)
					{
						returnCode = Convert.ToUInt32(Result);
					}
				}
			}
			catch (OleDbException exception)
			{
				log.Error(CultureInfo.InvariantCulture,
				m => m("Exception: {0} Command: {1}", exception.Message,
				sql));
				throw exception;
							}
			catch (InvalidOperationException exception)
			{
				log.Error(CultureInfo.InvariantCulture,
					m => m("Exception: {0} Command: {1}", exception.Message,
					sql));
				throw exception;
			}
			catch (Exception exception)
			{
				log.Error(CultureInfo.InvariantCulture,
					m => m("Exception: {0} Command: {1}", exception.Message,
					sql));
				throw exception;
			}
			finally
			{
				if (null == databaseTransaction)
				{
					Close();
				}

				if (null != command)
				{
					command.Dispose();
				}
			}
			return returnCode;
		}

		private DbCommand GetCommandObject(string sql)
		{
			DbCommand command = null;

			try
			{
				bool returnCode = Initialize();

				if (true == returnCode)
				{
					switch (databaseType)
					{
						case DatabaseTypes.OleDb:
						{
							command = new OleDbCommand();
							break;
						}
						case DatabaseTypes.SqlServer:
						{
							command = new SqlCommand();
							break;
						}
						case DatabaseTypes.MySql:
						{
							command = new MySqlCommand();
							break;
						}
					}

					command.Transaction = databaseTransaction;
					command.Connection = connection;
					command.CommandText = sql;
					command.CommandTimeout = 30;
				}
			}
			catch (Exception ex)
			{
				log.Error(CultureInfo.InvariantCulture,
					m => m("Initialization Error: {0}", ex.Message));
			}
			finally
			{
			}

			return command;
		}

		/// <summary>
		/// Prepares all necessary class variables by various constructors
		/// </summary>
		private bool Initialize(bool forceReset = false)
		{
			bool returnValue = false;

			try
			{
				if (true == forceReset)
				{
					if ((null != connection) &&
						(connection.State == ConnectionState.Open))
					{
						connection.Dispose();
						connection = null;
					}
				}

				if (null == connection)
				{
					switch (databaseType)
					{
						case DatabaseTypes.OleDb:
						{
							// Two statements help in debugging problems
							oleDbConnection = new OleDbConnection(connectionString);
							connection = oleDbConnection;
							break;
						}
						case DatabaseTypes.SqlServer:
						{
							connection = new SqlConnection(connectionString);
							break;
						}
						case DatabaseTypes.MySql:
						{
							mySqlConnection = new MySqlConnection(connectionString);
							connection = mySqlConnection;
							break;
						}
					}
				}

				if ((null != connection) &&
					(connection.State != ConnectionState.Open))
				{
					connection.Open();
				}

				returnValue = true;
			}
			catch (Exception ex)
			{
				log.Error(CultureInfo.InvariantCulture,
					m => m("Initialization Error: {0}", ex.Message));
				throw (ex);
			}
			finally
			{
			}

			return returnValue;
		}

		/// <summary>
		/// Sets an error message of an exception type for the log object.
		/// </summary>
		/// <param name="ex"></param>
		/// <param name="message"></param>
		/// <param name="command"></param>
		private void SetExceptionError(Exception ex, string message,
			string command)
		{
			log.Error(CultureInfo.InvariantCulture,
				m => m("Error: {0} Command: {1}", message, command));
			log.Error(CultureInfo.InvariantCulture,
				m => m("Initialization Error: {0}", ex.Message));
		}
	}	// end class
}	// end namespace
