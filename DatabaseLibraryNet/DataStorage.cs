/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataStorage.cs" company="James John McGuire">
// Copyright © 2006 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using DigitalZenWorks.Common.Utilities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;

namespace DigitalZenWorks.Database.ToolKit
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>DataStorage.</c>
	/// <summary>
	/// Class for Generic database access independent of the underlying
	/// transport.
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class DataStorage : IDisposable
	{
		/// <summary>
		/// Diagnostics object.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ResourceManager StringTable = new
			ResourceManager(
			"DigitalZenWorks.Database.ToolKit.Resources",
			Assembly.GetExecutingAssembly());

		/// <summary>
		/// databaseType.
		/// </summary>
		private readonly DatabaseType databaseType = DatabaseType.Unknown;

		/// <summary>
		/// The actual connection string used to connect to the database.
		/// </summary>
		private readonly string connectionText = string.Empty;

		/// <summary>
		/// Database Connection Object.
		/// </summary>
		private DbConnection connection;

		/// <summary>
		/// Ole Database Connection Object.
		/// </summary>
		private OleDbConnection oleDbConnection;

		private MySqlConnection mySqlConnection;

		private SQLiteConnection sqliteConnection;

		// transactions
		/// <summary>
		/// transaction object.
		/// </summary>
		private DbTransaction databaseTransaction;

		private int timeOut = 30;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataStorage"/> class.
		/// </summary>
		public DataStorage()
		{
			if ((ConfigurationManager.ConnectionStrings != null) &&
				(ConfigurationManager.ConnectionStrings.Count > 0))
			{
				connectionText =
					ConfigurationManager.ConnectionStrings[0].ConnectionString;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataStorage"/> class.
		/// </summary>
		/// <param name="databaseType">The database type.</param>
		/// <param name="connectionString">The connection string.</param>
		public DataStorage(DatabaseType databaseType, string connectionString)
		{
			this.databaseType = databaseType;
			connectionText = connectionString;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets get the table schema information for the associated database.
		/// </summary>
		/// <value>
		/// Get the table schema information for the associated database.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public DataTable SchemaTable
		{
			get
			{
				DataTable tables = null;

				bool returnCode = Initialize();

				if (true == returnCode)
				{
#if NET5_0_OR_GREATER
					if (DatabaseType.OleDb == databaseType &&
						OperatingSystem.IsWindows())
#else
					if (DatabaseType.OleDb == databaseType)
#endif
					{
						tables = oleDbConnection.GetOleDbSchemaTable(
							System.Data.OleDb.OleDbSchemaGuid.Tables,
							new object[] { null, null, null, "TABLE" });
					}
					else
					{
						tables = connection.GetSchema("Tables");
					}
				}

				return tables;
			}
		}

		/// <summary>
		/// Gets or sets or set the time out value.
		/// </summary>
		/// <value>
		/// The time out value.
		/// </value>
		public int TimeOut { get { return timeOut; } set { timeOut = value; } }

		/// <summary>
		/// Gets the database connection object.
		/// </summary>
		/// <value>
		/// The database connection object.
		/// </value>
		public DbConnection Connection
		{
			get { return connection; }
		}

		/// <summary>
		/// Converts a data table to a list.
		/// </summary>
		/// <typeparam name="TItem">The type of item.</typeparam>
		/// <param name="dataTable">The data table to convert.</param>
		/// <returns>Returns a list of items from the data table.</returns>
		public static Collection<TItem> ConvertDataTable<TItem>(DataTable dataTable)
		{
			Collection<TItem> list = new Collection<TItem>();

			if (dataTable != null)
			{
				foreach (DataRow row in dataTable.Rows)
				{
					TItem item = GetItem<TItem>(row);
					list.Add(item);
				}
			}

			return list;
		}

		/// <summary>
		/// Closes the database connection and object.
		/// </summary>
		public void Close()
		{
			Dispose(true);
		}

		/// <summary>
		/// Dispose.
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
			// System.GC.Collect();
		}

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
				Log.Error(CultureInfo.InvariantCulture, m => m(
					StringTable.GetString(
						"EXCEPTION",
						CultureInfo.InvariantCulture) + exception));
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
				databaseTransaction.Dispose();

				databaseTransaction = null;
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

		/////////////////////////////////////////////////////////////////////
		/// Method <c>CanQuery.</c>
		/// <summary>
		/// Checks to see if the database can return a valid query. Helper
		/// function for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise.</returns>
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
		/// Method <c>Delete.</c>
		/// <summary>
		/// Performs an SQL DELETE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>Success / Failure.</returns>
		/////////////////////////////////////////////////////////////////////
		public bool Delete(string sql)
		{
			bool returnCode = ExecuteNonQuery(sql);

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prepares and executes a Non-Query DB Command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>A value indicating success or not.</returns>
		/////////////////////////////////////////////////////////////////////
		public bool ExecuteNonQuery(string sql)
		{
			return ExecuteNonQuery(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Prepares and executes a Non-Query DB Command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>A value indicating success or not.</returns>
		/////////////////////////////////////////////////////////////////////
		public bool ExecuteNonQuery(
			string sql, IDictionary<string, object> values)
		{
			bool returnCode = false;

			using (DbCommand command = GetCommandObject(sql, values))
			{
				if (null != command)
				{
					try
					{
						int rowsEffected = command.ExecuteNonQuery();

						returnCode = true;
					}
					catch (Exception exception)
					{
						Log.Error(exception.ToString());

						throw;
					}
				}

				if (null == databaseTransaction)
				{
					Close();
				}
			}

			return returnCode;
		}

		/// <summary>
		/// Execture a data reader.
		/// </summary>
		/// <remarks>Caller is responsible for disposing returned
		/// object.</remarks>
		/// <param name="statement">the SQL statement to execute.</param>
		/// <returns>A data reader.</returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"StyleCop.CSharp.NamingRules",
			"SA1305:Field names should not use Hungarian notation",
			Justification = "It isn't hungarian notation.")]
		public DbDataReader ExecuteReader(string statement)
		{
			DbDataReader dbDataReader = null;

			using (DbCommand command = GetCommandObject(statement, null))
			{
				if (null != command)
				{
					dbDataReader = command.ExecuteReader();
				}
			}

			return dbDataReader;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>GetDataField.</c>
		/// <summary>
		/// Gets a single field from a row of data.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>the field object.</returns>
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
		/// Method <c>GetDataRow.</c>
		/// <summary>
		/// Gets a single row of data.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>DataRow.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataRow GetDataRow(string sql)
		{
			return GetDataRow(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>GetDataRow.</c>
		/// <summary>
		/// Gets a single row of data.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values of fields to get.</param>
		/// <returns>DataRow, null on failure.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataRow GetDataRow(
			string sql, IDictionary<string, object> values)
		{
			DataRow row = null;

			DataTable dataTable = GetDataTable(sql, values);

			if (dataTable != null && dataTable.Rows.Count > 0)
			{
				row = dataTable.Rows[0];
			}

			return row;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>DataSet or null on failure.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataSet GetDataSet(string sql)
		{
			return GetDataSet(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>DataSet or null on failure.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataSet GetDataSet(
			string sql, IDictionary<string, object> values)
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
							case DatabaseType.MySql:
								dataAdapter = new MySqlDataAdapter();
								break;
							case DatabaseType.OleDb:
#if NET5_0_OR_GREATER
								if (OperatingSystem.IsWindows())
								{
									dataAdapter = new OleDbDataAdapter();
								}
								else
								{
									throw new NotSupportedException(
										"OleDb is only available on Windows.");
								}
#else
								dataAdapter = new OleDbDataAdapter();
#endif

								break;
							case DatabaseType.SQLite:
								dataAdapter = new SQLiteDataAdapter();
								break;
							case DatabaseType.SqlServer:
								dataAdapter = new SqlDataAdapter();
								break;
						}

						if (dataAdapter != null)
						{
							dataAdapter.SelectCommand = command;
							dataAdapter.Fill(dataSet);
						}

						Log.Info(
							CultureInfo.InvariantCulture,
							m => m("OK - getDataSet - Query: {0}", sql));
					}
				}
			}
			catch (NullReferenceException exception)
			{
				RollbackTransaction();

				string message = StringTable.GetString(
					"EXCEPTION", CultureInfo.InvariantCulture);
				Log.Error(CultureInfo.InvariantCulture, m => m(
					message + exception));

				message = StringTable.GetString(
					"COMMAND", CultureInfo.InvariantCulture);
				Log.Error(CultureInfo.InvariantCulture, m => m(
					message + sql));
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				string message = StringTable.GetString(
					"EXCEPTION", CultureInfo.InvariantCulture);
				Log.Error(CultureInfo.InvariantCulture, m => m(
					message + exception));

				message = StringTable.GetString(
					"COMMAND", CultureInfo.InvariantCulture);
				Log.Error(CultureInfo.InvariantCulture, m => m(
					message + sql));
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
		/// GetDataTable.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>number of records retrieved.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetDataTable(string sql)
		{
			return GetDataTable(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// GetDataTable.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>DataTable or null on failure.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetDataTable(
			string sql, IDictionary<string, object> values)
		{
			DataTable dataTable = null;

			using (DataSet dataSet = GetDataSet(sql, values))
			{
				if (dataSet != null && dataSet.Tables.Count > 0)
				{
					dataTable = dataSet.Tables[0];
				}
			}

			return dataTable;
		}

		/// <summary>
		/// Get the last insert id.
		/// </summary>
		/// <returns>The last insert id.</returns>
		/// <exception cref="NotImplementedException">Throws a not implemented
		/// exception for databases that do not have this support.</exception>
		public int GetLastInsertId()
		{
			string statement;

			switch (databaseType)
			{
				case DatabaseType.OleDb:
				case DatabaseType.SqlServer:
					statement = "SELECT @@IDENTITY";
					break;
				case DatabaseType.SQLite:
					statement = "SELECT LAST_INSERT_ROWID()";
					break;
				default:
					throw new NotImplementedException();
			}

			int lastId = ExecuteScalar(statement);

			return lastId;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Insert.</c>
		/// <summary>
		/// Performs an Sql UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>object item.</returns>
		/////////////////////////////////////////////////////////////////////
		public int Insert(string sql)
		{
			return Insert(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Insert.</c>
		/// <summary>
		/// Performs an Sql UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>object item.</returns>
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
				returnCode = GetLastInsertId();

				if (true == finishTransaction)
				{
					CommitTransaction();
				}
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				Log.Error(CultureInfo.InvariantCulture, m => m(
					StringTable.GetString(
						"EXCEPTION",
						CultureInfo.InvariantCulture) + exception));
				Log.Error(CultureInfo.InvariantCulture, m => m(
					StringTable.GetString(
						"COMMAND",
						CultureInfo.InvariantCulture) + sql));

				throw;
			}

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>IsConnected.</c>
		/// <summary>
		/// Checks to see if the database is open and connected. Helper function
		/// for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise.</returns>
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

		/// <summary>
		/// Opens the database.
		/// </summary>
		/// <returns>A values indicating success or not.</returns>
		public bool Open()
		{
			return Initialize();
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Update.</c>
		/// <summary>
		/// Performs an Sql UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>A value indicating success or not.</returns>
		/////////////////////////////////////////////////////////////////////
		public bool Update(string sql)
		{
			return ExecuteNonQuery(sql, null);
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>Update.</c>
		/// <summary>
		/// Performs an SQL UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>A value indicating success or not.</returns>
		/////////////////////////////////////////////////////////////////////
		public bool Update(string sql, IDictionary<string, object> values)
		{
			return ExecuteNonQuery(sql, values);
		}

		/// <summary>
		/// Dispose.
		/// </summary>
		/// <param name="disposing">Indicates whether it is
		/// currently disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
#if NET5_0_OR_GREATER
				if (OperatingSystem.IsWindows())
				{
					if (null != oleDbConnection)
					{
						oleDbConnection.Close();
						oleDbConnection.Dispose();
						oleDbConnection = null;
					}
				}
#else
				if (null != oleDbConnection)
				{
					oleDbConnection.Close();
					oleDbConnection.Dispose();
					oleDbConnection = null;
				}
#endif

				if (null != mySqlConnection)
				{
					mySqlConnection.Close();
					mySqlConnection.Dispose();
					mySqlConnection = null;
				}

				if (null != sqliteConnection)
				{
					sqliteConnection.Close();
					sqliteConnection.Dispose();
					sqliteConnection = null;
				}

				if (null != databaseTransaction)
				{
					databaseTransaction.Dispose();
					databaseTransaction = null;
				}

				if (null != connection)
				{
					connection.Dispose();
					connection = null;
				}
			}
		}

#if NET5_0_OR_GREATER
		[SupportedOSPlatform("windows")]
#endif
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
				if (dataRow[column.ColumnName] != DBNull.Value)
				{
					PropertyInfo[] properitiesDetails =
						localType.GetProperties();

					foreach (PropertyInfo propertyDetails in
						properitiesDetails)
					{
						string singularPascalCase =
							GetSingularPascalName(column.ColumnName);

						if (propertyDetails.Name.Equals(
							column.ColumnName,
							StringComparison.OrdinalIgnoreCase) ||
							propertyDetails.Name.Equals(
							singularPascalCase,
							StringComparison.OrdinalIgnoreCase))
						{
							if (!propertyDetails.PropertyType.Name.Equals(
								column.DataType.Name,
								StringComparison.Ordinal))
							{
								var columnValue = Convert.ChangeType(
									dataRow[column.ColumnName],
									propertyDetails.PropertyType,
									CultureInfo.InvariantCulture);

								propertyDetails.SetValue(
									instance, columnValue, null);
							}
							else
							{
								propertyDetails.SetValue(
									instance, dataRow[column.ColumnName]);
							}

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

		private DbParameterCollection AddParameters(
			DbCommand command, IDictionary<string, object> values)
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
					object keyPairValue = valuePair.Value;

					if (databaseType == DatabaseType.SQLite)
					{
						SQLiteParameter parameter = new SQLiteParameter(
							DbType.String, keyPairValue);
						parameters.Add(parameter);
					}
					else
					{
						parameters.Add(keyPairValue);
					}
				}
			}

			return parameters;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The first column of the first row in the result set,
		/// or a null reference if the result set is empty.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private int ExecuteScalar(string sql)
		{
			int result = 0;

			using (DbCommand command = GetCommandObject(sql, null))
			{
				if (null != command)
				{
					object field = command.ExecuteScalar();

					if (null != field)
					{
						result = Convert.ToInt32(
							field, CultureInfo.InvariantCulture);
					}
				}

				if (null == databaseTransaction)
				{
					Close();
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Security",
			"CA2100:Review SQL queries for security vulnerabilities",
			Justification = "This is a generic function.  Evaluations should be done higher up the chain.")]
		private DbCommand GetCommandObject(
			string sql, IDictionary<string, object> values)
		{
			DbCommand command = null;

			try
			{
				bool returnCode = Initialize();

				if (true == returnCode)
				{
					switch (databaseType)
					{
						case DatabaseType.MySql:
							command = new MySqlCommand();
							break;
						case DatabaseType.OleDb:
#if NET5_0_OR_GREATER
							if (OperatingSystem.IsWindows())
							{
								command = new OleDbCommand();
							}
							else
							{
								throw new NotSupportedException(
									"OleDb is only available on Windows.");
							}
#else
							command = new OleDbCommand();
#endif

							break;
						case DatabaseType.SQLite:
							command = new SQLiteCommand();
							break;
						case DatabaseType.SqlServer:
							command = new SqlCommand();
							break;
					}

					if (null != values)
					{
						switch (databaseType)
						{
							case DatabaseType.OleDb:
								AddParameters((OleDbCommand)command, values);
								break;
							case DatabaseType.MySql:
							case DatabaseType.SQLite:
							case DatabaseType.SqlServer:
								AddParameters(command, values);
								break;
						}
					}

					command.Transaction = databaseTransaction;
					command.Connection = connection;
					command.CommandText = sql;
					command.CommandTimeout = timeOut;
				}
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				Log.Error(CultureInfo.InvariantCulture, m => m(
					StringTable.GetString(
						"EXCEPTION",
						CultureInfo.InvariantCulture) + exception));
				Log.Error(CultureInfo.InvariantCulture, m => m(
					StringTable.GetString(
						"COMMAND",
						CultureInfo.InvariantCulture) + sql));

				throw;
			}

			return command;
		}

		/// <summary>
		/// Prepares all necessary class variables by various constructors.
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
						case DatabaseType.MySql:
							mySqlConnection =
								new MySqlConnection(connectionText);
							connection = mySqlConnection;
							break;
						case DatabaseType.OleDb:
#if NET5_0_OR_GREATER
							if (OperatingSystem.IsWindows())
							{
								// Two statements help in debugging problems
								oleDbConnection =
									new OleDbConnection(connectionText);
								connection = oleDbConnection;
							}
							else
							{
								throw new NotSupportedException(
									"OleDb is only available on Windows.");
							}
#else
							// Two statements help in debugging problems
							oleDbConnection =
								new OleDbConnection(connectionText);
							connection = oleDbConnection;
#endif

							break;
						case DatabaseType.SQLite:
							sqliteConnection =
								new SQLiteConnection(connectionText);
							connection = sqliteConnection;
							break;
						case DatabaseType.SqlServer:
							connection =
								new SqlConnection(connectionText);
							break;
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
				(exception is AccessViolationException ||
				exception is OleDbException)
			{
				RollbackTransaction();

				Log.Error(CultureInfo.InvariantCulture, m => m(
					StringTable.GetString(
						"EXCEPTION",
						CultureInfo.InvariantCulture) + exception));
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				Log.Error(CultureInfo.InvariantCulture, m => m(
					StringTable.GetString(
						"EXCEPTION",
						CultureInfo.InvariantCulture) + exception));

				throw;
			}

			return returnValue;
		}
	} // end class
} // end namespace
