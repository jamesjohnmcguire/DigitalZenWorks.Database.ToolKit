/////////////////////////////////////////////////////////////////////////////
// $Id: DataStorage.cs 42 2015-06-04 14:48:09Z JamesMc $
//
// Copyright © 2006 - 2016 by James John McGuire
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
using System.Reflection;
using System.Resources;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>DataStorage</c>
	/// <summary>
	/// Class for Generic database access independent of the underlying
	/// transport
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class DataStorage: IDisposable
	{
		#region private variables

		/// <summary>
		/// databaseType
		/// </summary>
		private DatabaseType databaseType;

		/// <summary>
		/// The actual connection string used to connect to the database.
		/// </summary>
		private string connectionText = string.Empty;

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

		private static readonly ResourceManager stringTable = new
			ResourceManager("DigitalZenWorks.Common.DatabaseLibrary.Resources",
			Assembly.GetExecutingAssembly());
		#endregion private variables

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Get the table schema information for the associated database.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public DataTable SchemaTable
		{
			get
			{
				DataTable tables = null;

				bool returnCode = Initialize();

				if (true == returnCode)
				{
					if (DatabaseType.OleDb == databaseType)
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
		}

		#region constructors

		/// <summary>
		/// DataStorage - Default constructor
		/// </summary>
		public DataStorage()
		{
			if ((ConfigurationManager.ConnectionStrings != null) &&
				(ConfigurationManager.ConnectionStrings.Count > 0))
			{
				connectionText =
					ConfigurationManager.ConnectionStrings[0].ConnectionString;

				// OleDbConnection is default
				databaseType = DatabaseType.OleDb;
			}
		}

		/// <summary>
		/// DataStorage - Constructor
		/// </summary>
		/// <param name="provider"></param>
		/// <param name="dataSource"></param>
		public DataStorage(string provider, string dataSource)
		{
			this.provider = provider;
			connectionText = CreateConnectionString(dataSource, null);
			databaseType = DatabaseType.OleDb;
		}

		/// <summary>
		/// DataStorage - Constructor
		/// </summary>
		/// <param name="databaseType"></param>
		/// <param name="dataSource"></param>
		/// <param name="catalog"></param>
		public DataStorage(DatabaseType databaseType, string dataSource,
			string catalog)
		{
			connectionText = CreateConnectionString(dataSource, catalog);
			this.databaseType = databaseType;
		}

		/// <summary>
		/// DataStorage - Constructor
		/// </summary>
		/// <param name="databaseType"></param>
		/// <param name="connectionString"></param>
		public DataStorage(DatabaseType databaseType, string connectionString)
		{
			connectionText = connectionString;
			this.databaseType = databaseType;
		}

		#endregion constructors

		#region startup and shutdown

		/// <summary>
		/// Closes the database connection and object
		/// </summary>
		public void Close()
		{
			Dispose(true);
		}

		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
			if (null != connection)
			{
				connection.Dispose();
				connection = null;
			}

				if (null != oleDbConnection)
				{
					oleDbConnection.Close();
					oleDbConnection = null;
		}

				if (null != mySqlConnection)
				{
					mySqlConnection.Close();
					mySqlConnection = null;
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

		/// <summary>
		/// The database connection object
		/// </summary>
		public DbConnection Connection
		{
			get { return connection; }
		}

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
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is OleDbException ||
				exception is InvalidOperationException)
			{
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception.Message));
			}
			catch
			{
				throw;
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

			DataTable tables = SchemaTable;

			if ((null != tables) && (tables.Rows.Count > 0))
			{
				canQuery = true;
			}

			return canQuery;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Delete</c>
		/// <summary>
		/// Performs an SQL DELETE command
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

			using (DbCommand command = GetCommandObject(sql))
			{
				if (null != command)
				{
					int rowsEffected = command.ExecuteNonQuery();

					if (rowsEffected > 0)
					{
						returnCode = true;
					}
				}

				if (null == databaseTransaction)
				{
					Close();
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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
			"CA1021:AvoidOutParameters", MessageId = "1#")]
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
		/// <param name="dataSet"></param>
		/// <returns>number of records retrieved</returns>
		/////////////////////////////////////////////////////////////////////
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
			"CA1021:AvoidOutParameters", MessageId = "1#")]
		public int GetDataSet(string sql, out DataSet dataSet)
		{
			int rowCount = -1;
			dataSet = null;
			DbDataAdapter dataAdapter = null;

			try
			{
				dataSet = new DataSet();
				dataSet.Locale = CultureInfo.InvariantCulture;

				using (DbCommand command = GetCommandObject(sql))
				{
					if (null != command)
					{
						switch (databaseType)
						{
							case DatabaseType.OleDb:
								{
									dataAdapter = new OleDbDataAdapter();
									break;
								}
							case DatabaseType.SqlServer:
								{
									dataAdapter = new SqlDataAdapter();
									break;
								}
							case DatabaseType.MySql:
								{
									dataAdapter = new MySqlDataAdapter();
									break;
								}
						}

						dataAdapter.SelectCommand = command;

						rowCount = dataAdapter.Fill(dataSet);

						log.Info(CultureInfo.InvariantCulture,
							m => m("OK - getDataSet - Query: {0}", sql));
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is OleDbException ||
				exception is InvalidOperationException)
			{
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception.Message));
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("COMMAND") + sql));
			}
			catch
			{
				throw;
			}
			finally
			{
				if (null != dataAdapter)
				{
					dataAdapter.Dispose();
					dataAdapter = null;
				}

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
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design",
			"CA1021:AvoidOutParameters", MessageId = "1#")]
		public int GetDataTable(string sql, out DataTable dataTable)
		{
			int recordsReturned = -1;
			DataSet dataSet = null;

			dataTable = new DataTable();
			dataTable.Locale = CultureInfo.InvariantCulture;

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
		[CLSCompliantAttribute(false)]
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
		public int Test()
		{
			int returnCode = -1;
			OleDbCommand commandObject1 = null;
			OleDbCommand commandObject2 = null;

			try
			{
				Initialize();

				string sql =
					"INSERT INTO Contacts (Notes) VALUES ('testing')";
				string Sql2 = "SELECT @@IDENTITY";

				commandObject1 = new OleDbCommand(sql, oleDbConnection);
				commandObject2 = new OleDbCommand(Sql2, oleDbConnection);

				oleDbConnection.Open();
				//CommandObject.CommandTimeout = 30;

				Object result = commandObject1.ExecuteScalar();

				if (null != result)
				{
					returnCode = (int)result;
				}

				returnCode = (int)commandObject2.ExecuteScalar();
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is OleDbException ||
				exception is InvalidOperationException)
			{
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception.Message));
			}
			catch
			{
				throw;
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

			return returnCode;
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
		/// or a null reference if the result set is empty
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private uint ExecuteScalar(string sql)
		{
			uint returnCode = 0;

			using (DbCommand command = GetCommandObject(sql))
			{
				if (null != command)
				{
					Object Result = command.ExecuteScalar();

					if (null != Result)
					{
						returnCode = Convert.ToUInt32(Result,
							CultureInfo.InvariantCulture);
					}
				}

				if (null == databaseTransaction)
				{
					Close();
				}
			}

			return returnCode;
		}

		//private DbCommand GetCommandObject(string sql,
		//	DbParameterCollection parameters)
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope")]
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
						case DatabaseType.OleDb:
						{
							command = new OleDbCommand();
							break;
						}
						case DatabaseType.SqlServer:
						{
							command = new SqlCommand();
							break;
						}
						case DatabaseType.MySql:
						{
							command = new MySqlCommand();
							break;
						}
					}

					//foreach (DbParameter parameter in parameters)
					//{
					//	command.Parameters.Add(parameter);
					//}

					command.Transaction = databaseTransaction;
					command.Connection = connection;
				command.CommandText = sql;
					command.CommandTimeout = 30;
				}
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is OleDbException ||
				exception is InvalidOperationException)
			{
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception.Message));
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("COMMAND") + sql));
			}
			catch
			{
				throw;
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
						case DatabaseType.OleDb:
						{
							// Two statements help in debugging problems
							oleDbConnection = new OleDbConnection(connectionText);
							connection = oleDbConnection;
							break;
						}
						case DatabaseType.SqlServer:
						{
							connection = new SqlConnection(connectionText);
							break;
						}
						case DatabaseType.MySql:
						{
							mySqlConnection = new MySqlConnection(connectionText);
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
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is OleDbException ||
				exception is InvalidOperationException)
			{
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception.Message));
			}
			catch
			{
				throw;
			}
			finally
			{
			}

			return returnValue;
		}
	}	// end class
}	// end namespace
