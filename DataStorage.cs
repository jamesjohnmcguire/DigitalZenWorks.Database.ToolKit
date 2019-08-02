/////////////////////////////////////////////////////////////////////////////
// Copyright © 2006 - 2019 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using Common.Logging;
using DigitalZenWorks.Common.Utilities;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
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
		private readonly DatabaseType databaseType;

		/// <summary>
		/// The actual connection string used to connect to the database.
		/// </summary>
		private readonly string connectionText = string.Empty;

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

		private readonly string provider = string.Empty;

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
		/// <param name="connectionString"></param>
		public DataStorage(DatabaseType databaseType, string connectionString)
		{
			connectionText = connectionString;
			this.databaseType = databaseType;
		}

		/// <summary>
		/// DataStorage - Constructor
		/// </summary>
		/// <param name="databaseType"></param>
		/// <param name="dataSource"></param>
		/// <param name="catalog"></param>
		public DataStorage(
			DatabaseType databaseType, string dataSource, string catalog)
		{
			connectionText = CreateConnectionString(dataSource, catalog);
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

				if (null != databaseTransaction)
				{
					databaseTransaction.Dispose();
					databaseTransaction = null;
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
			catch (Exception exception)
			{
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception));
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

		public static List<TItem> ConvertDataTable<TItem>(DataTable dataTable)
		{
			List<TItem> list = new List<TItem>();
			foreach (DataRow row in dataTable.Rows)
			{
				TItem item = GetItem<TItem>(row);
				list.Add(item);
			}

			return list;
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
			return ExecuteNonQuery(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prepares and executes a Non-Query DB Command
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public bool ExecuteNonQuery(string sql,
			IDictionary<string, object> values)
		{
			bool returnCode = false;

			using (DbCommand command = GetCommandObject(sql, values))
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
			DataRow dataRowOut = GetDataRow(sql);

			if (null != dataRowOut)
			{
				datafield = dataRowOut.ItemArray[0];
			}

			return datafield;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>GetDataRow</c>
		/// <summary>
		/// Gets a single row of data
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>DataRow</returns>
		/////////////////////////////////////////////////////////////////////
		public DataRow GetDataRow(string sql)
		{
			return GetDataRow(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>GetDataRow</c>
		/// <summary>
		/// Gets a single row of data
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="values"></param>
		/// <returns>DataRow, null on failure</returns>
		/////////////////////////////////////////////////////////////////////
		public DataRow GetDataRow(string sql,
			IDictionary<string, object> values)
		{
			DataRow row = null;

			DataTable dataTable = GetDataTable(sql, values);

			if (dataTable.Rows.Count > 0)
			{
				row = dataTable.Rows[0];
			}

			return row;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>DataSet or null on failure</returns>
		/////////////////////////////////////////////////////////////////////
		public DataSet GetDataSet(string sql)
		{
			return GetDataSet(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="values"></param>
		/// <returns>DataSet or null on failure</returns>
		/////////////////////////////////////////////////////////////////////
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope")]
		public DataSet GetDataSet(string sql,
			IDictionary<string, object> values)
		{
			DataSet dataSet = null;
			DbDataAdapter dataAdapter = null;

			try
			{
				dataSet = new DataSet();
				dataSet.Locale = CultureInfo.InvariantCulture;

				using (DbCommand command = GetCommandObject(sql, values))
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
						dataAdapter.Fill(dataSet);

						log.Info(CultureInfo.InvariantCulture,
							m => m("OK - getDataSet - Query: {0}", sql));
					}
				}
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception));
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("COMMAND") + sql));
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

			return dataSet;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// GetDataTable
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>number of records retrieved</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetDataTable(string sql)
		{
			return GetDataTable(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// GetDataTable
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="values"></param>
		/// <returns>DataTable or null on failure</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetDataTable(string sql,
			IDictionary<string, object> values)
		{
			DataTable dataTable = null;
			//dataTable.Locale = CultureInfo.InvariantCulture;

			DataSet dataSet = GetDataSet(sql, values);
			if (dataSet.Tables.Count > 0)
			{
				dataTable = dataSet.Tables[0];
			}

			return dataTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Insert</c>
		/// <summary>
		/// Performs an Sql UPDATE command
		/// </summary>
		/// <param name="sql"></param>
		/// <returns>object item</returns>
		/////////////////////////////////////////////////////////////////////
		public int Insert(string sql)
		{
			return Insert(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Insert</c>
		/// <summary>
		/// Performs an Sql UPDATE command
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="values"></param>
		/// <returns>object item</returns>
		/////////////////////////////////////////////////////////////////////
		public int Insert(string sql, IDictionary<string, object> values)
		{
			int returnCode = 0;

			try
			{
				bool finishTransaction = false;
				if (null == databaseTransaction)
				{
					BeginTransaction();
					finishTransaction = true;
				}

				// execute non query
				ExecuteNonQuery(sql, values);

				// get id of effected row
				returnCode = ExecuteScalar("SELECT @@IDENTITY");

				if (true == finishTransaction)
				{
					CommitTransaction();
				}
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception));
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("COMMAND") + sql));

				throw;
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
			return ExecuteNonQuery(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Update</c>
		/// <summary>
		/// Performs an SQL UPDATE command
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="values"></param>
		/// <returns>Success / Failure</returns>
		/////////////////////////////////////////////////////////////////////
		public bool Update(string sql, IDictionary<string, object> values)
		{
			return ExecuteNonQuery(sql, values);
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
			catch (Exception exception)
			{
				RollbackTransaction();

				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception));

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

		private static DbParameterCollection AddParameters(DbCommand command,
			IDictionary<string, object> values)
		{
			DbParameterCollection parameters = command.Parameters;

			foreach (KeyValuePair<string, object> valuePair in values)
			{
				if (null == valuePair.Value)
				{
					parameters.Add(DBNull.Value);
				}
				else
				{
					parameters.Add(valuePair.Value);
				}
			}

			return parameters;
		}

		private static OleDbParameterCollection AddParameters(
			OleDbCommand command, IDictionary<string, object> values)
		{
			OleDbParameterCollection parameters = command.Parameters;

			foreach (KeyValuePair<string, object> valuePair in values)
			{
				string name = "@" + valuePair.Key;

				if (null == valuePair.Value)
				{
					parameters.AddWithValue(name, DBNull.Value);
				}
				else
				{
					parameters.AddWithValue(name, valuePair.Value);
				}
			}

			return parameters;
		}

		private static TItem GetItem<TItem>(DataRow dataRow)
		{
			Type localType = typeof(TItem);
			TItem instance = Activator.CreateInstance<TItem>();

			foreach (DataColumn column in dataRow.Table.Columns)
			{
				bool found = false;

				if (dataRow[column.ColumnName] != DBNull.Value)
				{
					PropertyInfo[] properitiesDetails =
						localType.GetProperties();

					foreach (PropertyInfo propertyDetails in
						properitiesDetails)
					{
						string singularPascalCase =
							GetSingularPascalName(column.ColumnName);

						if ((propertyDetails.Name.Equals(
							column.ColumnName,
							StringComparison.OrdinalIgnoreCase)) ||
							(propertyDetails.Name.Equals(
							singularPascalCase,
							StringComparison.OrdinalIgnoreCase)))
						{
							if (!propertyDetails.PropertyType.Name.Equals(
								column.DataType.Name))
							{
								var columnValue = Convert.ChangeType(
									dataRow[column.ColumnName],
									propertyDetails.PropertyType);

								propertyDetails.SetValue(
									instance, columnValue, null);
							}
							else
							{
								propertyDetails.SetValue(
									instance, dataRow[column.ColumnName]);
							}

							found = true;
							break;
						}
					}
				}
			}

			return instance;
		}

		private static string GetSingularPascalName(string columnName)
		{
			return GeneralUtilities.ConvertToPascalCaseFromKnr(columnName);
		}

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
		private int ExecuteScalar(string sql)
		{
			int result = 0;

			using (DbCommand command = GetCommandObject(sql, null))
			{
				if (null != command)
				{
					Object Result = command.ExecuteScalar();

					if (null != Result)
					{
						result = Convert.ToInt32(Result,
							CultureInfo.InvariantCulture);
					}
				}

				if (null == databaseTransaction)
				{
					Close();
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security",
			"CA2100:Review SQL queries for security vulnerabilities")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope")]
		private DbCommand GetCommandObject(string sql,
			IDictionary<string, object> values)
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

					if (null != values)
					{
						switch (databaseType)
						{
						case DatabaseType.OleDb:
							{
								AddParameters((OleDbCommand)command, values);
								break;
							}
						case DatabaseType.SqlServer:
						case DatabaseType.MySql:
							{
								AddParameters(command, values);
								break;
							}
						}
					}

					command.Transaction = databaseTransaction;
					command.Connection = connection;
					command.CommandText = sql;
					command.CommandTimeout = 30;
				}
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception));
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("COMMAND") + sql));

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
			catch (Exception exception)
			{
				RollbackTransaction();

				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception));

				throw;
			}
			finally
			{
			}

			return returnValue;
		}
	}	// end class
}	// end namespace
