/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataStorage.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Configuration;
	using System.Data;
	using System.Data.Common;
	using System.Data.SQLite;
	using System.Globalization;
	using System.Reflection;
	using DigitalZenWorks.Common.Utilities;
	using global::Common.Logging;
	using Microsoft.Data.SqlClient;
	using MySql.Data.MySqlClient;

	/// Class <c>DataStorage.</c>
	/// <summary>
	/// Class for Generic database access independent of the underlying
	/// transport.
	/// </summary>
	public class DataStorage : IDataStorage
	{
		/// <summary>
		/// Diagnostics object.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// databaseType.
		/// </summary>
		private readonly DatabaseType databaseType = DatabaseType.Unknown;

		/// <summary>
		/// The actual connection string used to connect to the database.
		/// </summary>
		private string connectionText = string.Empty;

		private DbDataAdapter dataAdapter;

		private MySqlConnection mySqlConnection;

		private SQLiteConnection sqliteConnection;

		/// <summary>
		/// transaction object.
		/// </summary>
		private DbTransaction databaseTransaction;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataStorage"/> class.
		/// </summary>
		public DataStorage()
		{
			if (ConfigurationManager.ConnectionStrings?.Count > 0)
			{
				connectionText =
					ConfigurationManager.ConnectionStrings[0].ConnectionString;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataStorage"/> class.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		public DataStorage(string connectionString)
		{
			connectionText = connectionString;
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

		/// <summary>
		/// Gets get the table schema information for the associated database.
		/// </summary>
		/// <value>
		/// Get the table schema information for the associated database.
		/// </value>
		public virtual DataTable SchemaTable
		{
			get
			{
				DataTable tables = null;

				bool returnCode = Initialize();

				if (returnCode == true)
				{
					tables = GetTables();
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
		public int TimeOut { get; set; } = 30;

		/// <summary>
		/// Gets the database command object.
		/// </summary>
		/// <value>The database command object.</value>
		public virtual DbCommand Command
		{
			get
			{
				DbCommand command = null;

				switch (databaseType)
				{
					case DatabaseType.MySql:
						command = new MySqlCommand();
						break;
					case DatabaseType.SQLite:
						command = new SQLiteCommand();
						break;
					case DatabaseType.SqlServer:
						command = new SqlCommand();
						break;
					case DatabaseType.Oracle:
					case DatabaseType.Unknown:
					default:
						break;
				}

				return command;
			}
		}

		/// <summary>
		/// Gets the database connection object.
		/// </summary>
		/// <value>
		/// The database connection object.
		/// </value>
		public DbConnection Connection { get; private set; }

		/// <summary>
		/// Gets or sets the connection text.
		/// </summary>
		/// <value>The connection text.</value>
		public virtual string ConnectionText
		{
			get { return connectionText; }
			set { connectionText = value; }
		}

		/// <summary>
		/// Gets or sets the data adapter object.
		/// </summary>
		/// <value>The data adapter object.</value>
		public virtual DbDataAdapter DataAdapter
		{
			get
			{
				switch (databaseType)
				{
					case DatabaseType.MySql:
						dataAdapter = new MySqlDataAdapter();
						break;
					case DatabaseType.SQLite:
						dataAdapter = new SQLiteDataAdapter();
						break;
					case DatabaseType.SqlServer:
						dataAdapter = new SqlDataAdapter();
						break;
					case DatabaseType.Oracle:
					case DatabaseType.Unknown:
					default:
						break;
				}

				return dataAdapter;
			}

			set => dataAdapter = value;
		}

		/// <summary>
		/// Generates a database connection string for the specified database
		/// type and data source.
		/// </summary>
		/// <remarks>The returned connection string uses default credentials
		/// and settings. If an unsupported database type is specified, the
		/// method returns null.</remarks>
		/// <param name="databaseType">The type of database for which to
		/// generate the connection string. Supported values include MySql,
		/// OleDb, and SQLite.</param>
		/// <param name="dataSource">The data source identifier, such as a
		/// server name, file path, or connection endpoint, used to construct
		/// the connection string. Cannot be null.</param>
		/// <returns>A connection string formatted for the specified database
		/// type and data source. Returns null if the database type is not
		/// supported.</returns>
		public static string GetConnectionString(
			DatabaseType databaseType, string dataSource)
		{
			string connectionString = null;

			switch (databaseType)
			{
				case DatabaseType.MySql:
					connectionString = $"Server={dataSource};" +
						"Database=your_database;Uid=your_username;" +
						"Pwd=your_password;";
					break;
				case DatabaseType.SQLite:
					const string connectionBase = "Data Source={0};Version=3;" +
						"DateTimeFormat=InvariantCulture";
					connectionString = string.Format(
						CultureInfo.InvariantCulture,
						connectionBase,
						dataSource);
					break;
				case DatabaseType.SqlServer:
				case DatabaseType.Oracle:
				case DatabaseType.Unknown:
				default:
					break;
			}

			return connectionString;
		}

		/// <summary>
		/// Converts a data table to a list.
		/// </summary>
		/// <typeparam name="TItem">The type of item.</typeparam>
		/// <param name="dataTable">The data table to convert.</param>
		/// <returns>Returns a list of items from the data table.</returns>
		public Collection<TItem> ConvertDataTable<TItem>(DataTable dataTable)
		{
			Collection<TItem> list = [];

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

		/// <summary>
		/// Shut down the database.
		/// </summary>
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

				if (returnCode == true)
				{
					databaseTransaction = Connection.BeginTransaction();
				}
			}
			catch (Exception exception)
			{
				string message = Strings.Exception + exception;
				Log.Error(message);

				throw;
			}
		}

		/// <summary>
		/// This closes the transaction.
		/// </summary>
		public void CloseTransaction()
		{
			databaseTransaction?.Connection.Close();
		}

		/// <summary>
		/// This commits the transaction.
		/// </summary>
		public void CommitTransaction()
		{
			if (databaseTransaction != null)
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
			databaseTransaction?.Rollback();
		}

		/// Method <c>CanQuery.</c>
		/// <summary>
		/// Checks to see if the database can return a valid query. Helper
		/// function for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise.</returns>
		public bool CanQuery()
		{
			bool canQuery = false;

			DataTable tables = SchemaTable;

			if (tables?.Rows.Count > 0)
			{
				canQuery = true;
			}

			return canQuery;
		}

		/// Method <c>Delete.</c>
		/// <summary>
		/// Performs an SQL DELETE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>Success / Failure.</returns>
		public bool Delete(string sql)
		{
			bool returnCode = ExecuteNonQuery(sql);

			return returnCode;
		}

		/// <summary>
		/// Prepares and executes a Non-Query DB Command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>A value indicating success or not.</returns>
		public bool ExecuteNonQuery(string sql)
		{
			return ExecuteNonQuery(sql, null);
		}

		/// <summary>
		/// Prepares and executes a Non-Query DB Command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>A value indicating success or not.</returns>
		public bool ExecuteNonQuery(
			string sql, IDictionary<string, object> values)
		{
			bool returnCode = false;

			using (DbCommand command = GetCommandObject(sql, values))
			{
				if (command != null)
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

				if (databaseTransaction == null)
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
		public DbDataReader ExecuteReader(string statement)
		{
			DbDataReader dbDataReader = null;

			using (DbCommand command = GetCommandObject(statement, null))
			{
				if (command != null)
				{
					dbDataReader = command.ExecuteReader();
				}
			}

			return dbDataReader;
		}

		/// Method <c>GetDataField.</c>
		/// <summary>
		/// Gets a single field from a row of data.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>the field object.</returns>
		public object GetDataField(string sql)
		{
			object datafield = null;
			DataRow dataRowOut = GetDataRow(sql);

			if (dataRowOut != null)
			{
				datafield = dataRowOut.ItemArray[0];
			}

			return datafield;
		}

		/// Method <c>GetDataRow.</c>
		/// <summary>
		/// Gets a single row of data.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>DataRow.</returns>
		public DataRow GetDataRow(string sql)
		{
			return GetDataRow(sql, null);
		}

		/// Method <c>GetDataRow.</c>
		/// <summary>
		/// Gets a single row of data.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values of fields to get.</param>
		/// <returns>DataRow, null on failure.</returns>
		public DataRow GetDataRow(
			string sql, IDictionary<string, object> values)
		{
			DataRow row = null;

			DataTable dataTable = GetDataTable(sql, values);

			if (dataTable?.Rows.Count > 0)
			{
				row = dataTable.Rows[0];
			}

			return row;
		}

		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>DataSet or null on failure.</returns>
		public DataSet GetDataSet(string sql)
		{
			return GetDataSet(sql, null);
		}

		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>DataSet or null on failure.</returns>
		public DataSet GetDataSet(
			string sql, IDictionary<string, object> values)
		{
			DataSet dataSet = null;

			try
			{
				dataSet = new DataSet();
				dataSet.Locale = CultureInfo.InvariantCulture;

				using DbCommand command = GetCommandObject(sql, values);

				if (command != null)
				{
					DbDataAdapter dataAdapter = DataAdapter;

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
			catch (NullReferenceException exception)
			{
				RollbackTransaction();

				string message = Strings.Exception + exception;
				Log.Error(message);

				message = Strings.Command + sql;
				Log.Error(message);
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				string message = Strings.Exception + exception;
				Log.Error(message);

				message = Strings.Command + sql;
				Log.Error(message);

				throw;
			}
			finally
			{
				if (databaseTransaction == null)
				{
					Close();
				}
			}

			return dataSet;
		}

		/// <summary>
		/// GetDataTable.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>number of records retrieved.</returns>
		public DataTable GetDataTable(string sql)
		{
			return GetDataTable(sql, null);
		}

		/// <summary>
		/// GetDataTable.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>DataTable or null on failure.</returns>
		public DataTable GetDataTable(
			string sql, IDictionary<string, object> values)
		{
			DataTable dataTable = null;

			using DataSet dataSet = GetDataSet(sql, values);

			if (dataSet?.Tables.Count > 0)
			{
				dataTable = dataSet.Tables[0];
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
			int lastId = -1;
			string statement = null;

			switch (databaseType)
			{
				case DatabaseType.OleDb:
				case DatabaseType.SqlServer:
					statement = "SELECT @@IDENTITY";
					break;
				case DatabaseType.SQLite:
					statement = "SELECT LAST_INSERT_ROWID()";
					break;
				case DatabaseType.Unknown:
					break;
				case DatabaseType.Oracle:
					break;
				case DatabaseType.MySql:
					break;
				default:
					throw new NotImplementedException();
			}

			if (statement != null)
			{
				lastId = ExecuteScalar(statement);
			}

			return lastId;
		}

		/// Method <c>Insert.</c>
		/// <summary>
		/// Performs an Sql UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>object item.</returns>
		public int Insert(string sql)
		{
			return Insert(sql, null);
		}

		/// Method <c>Insert.</c>
		/// <summary>
		/// Performs an Sql UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>object item.</returns>
		public int Insert(string sql, IDictionary<string, object> values)
		{
			int returnCode;

			try
			{
				bool finishTransaction = false;
				if (databaseTransaction == null)
				{
					BeginTransaction();
					finishTransaction = true;
				}

				// execute non query
				bool result = ExecuteNonQuery(sql, values);

				if (result == false)
				{
					Log.Warn("ExecuteNonQuery returns false");
				}

				// get id of effected row
				returnCode = GetLastInsertId();

				if (finishTransaction == true)
				{
					CommitTransaction();
				}
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				string message = Strings.Exception + exception;
				Log.Error(message);

				message = Strings.Command + sql;
				Log.Error(message);

				throw;
			}

			return returnCode;
		}

		/// Method <c>IsConnected.</c>
		/// <summary>
		/// Checks to see if the database is open and connected. Helper function
		/// for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise.</returns>
		public bool IsConnected()
		{
			bool connected = false;

			if ((Connection != null) &&
				(Connection.State == ConnectionState.Open))
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

		/// Method <c>Update.</c>
		/// <summary>
		/// Performs an Sql UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>A value indicating success or not.</returns>
		public bool Update(string sql)
		{
			return ExecuteNonQuery(sql, null);
		}

		/// Method <c>Update.</c>
		/// <summary>
		/// Performs an SQL UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>A value indicating success or not.</returns>
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
				dataAdapter?.Dispose();
				dataAdapter = null;

				if (mySqlConnection != null)
				{
					mySqlConnection.Close();
					mySqlConnection.Dispose();
					mySqlConnection = null;
				}

				if (sqliteConnection != null)
				{
					sqliteConnection.Close();
					sqliteConnection.Dispose();
					sqliteConnection = null;
				}

				databaseTransaction?.Dispose();
				databaseTransaction = null;
				Connection?.Dispose();
				Connection = null;
			}
		}

		/// <summary>
		/// Gets the database connection object.
		/// </summary>
		/// <param name="databaseType">The database type.</param>
		/// <param name="connectionText">The connection text.</param>
		/// <returns>A DbConnection object or null.</returns>
		protected virtual DbConnection GetConnectionObject(
			DatabaseType databaseType, string connectionText)
		{
			DbConnection connection = null;

			switch (databaseType)
			{
				case DatabaseType.MySql:
					MySqlConnection mySqlConnection = new(connectionText);
					connection = mySqlConnection;
					break;
				case DatabaseType.SQLite:
					SQLiteConnection sqliteConnection = new(connectionText);
					connection = sqliteConnection;
					break;
				case DatabaseType.SqlServer:
					connection = new SqlConnection(connectionText);
					break;
				case DatabaseType.Oracle:
				case DatabaseType.Unknown:
				default:
					break;
			}

			return connection;
		}

		/// <summary>
		/// Gets the table schema information for the associated database.
		/// </summary>
		/// <returns>The table of tables.</returns>
		protected virtual DataTable GetTables()
		{
			DataTable tables = Connection.GetSchema("Tables");

			return tables;
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
								object columnValue = Convert.ChangeType(
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
			string result = TextCase.ConvertToPascalCaseFromKnr(columnName);

			return result;
		}

		private DbParameterCollection AddParameters(
			DbCommand command, IDictionary<string, object> values)
		{
			DbParameterCollection parameters = command.Parameters;

			foreach (KeyValuePair<string, object> valuePair in values)
			{
				int result;

				if (valuePair.Value == null)
				{
					result = parameters.Add(DBNull.Value);
				}
				else
				{
					// DbParameter is abstract, so we have to create one of the
					// specific types below.
					string keyPairName = valuePair.Key;
					object keyPairValue = valuePair.Value;

					if (databaseType == DatabaseType.SQLite)
					{
						SQLiteParameter parameter =
							new(DbType.String, keyPairValue);
						parameter.ParameterName = keyPairName;
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

		/// <summary>
		/// The first column of the first row in the result set,
		/// or a null reference if the result set is empty.
		/// </summary>
		private int ExecuteScalar(string sql)
		{
			int result = 0;

			using (DbCommand command = GetCommandObject(sql, null))
			{
				if (command != null)
				{
					object field = command.ExecuteScalar();

					if (field != null)
					{
						result = Convert.ToInt32(
							field, CultureInfo.InvariantCulture);
					}
				}

				if (databaseTransaction == null)
				{
					Close();
				}
			}

			return result;
		}

		private DbCommand GetCommandObject(
			string sql, IDictionary<string, object> values)
		{
			DbCommand command = null;

			try
			{
				bool returnCode = Initialize();

				if (returnCode == true)
				{
					command = Command;

					if (values != null)
					{
						switch (databaseType)
						{
							case DatabaseType.MySql:
							case DatabaseType.SQLite:
							case DatabaseType.SqlServer:
								AddParameters(command, values);
								break;
							case DatabaseType.Oracle:
							case DatabaseType.Unknown:
							default:
								break;
						}
					}

					command.Transaction = databaseTransaction;
					command.Connection = Connection;
					command.CommandText = sql;
					command.CommandTimeout = TimeOut;
				}
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				string message = Strings.Exception + exception;
				Log.Error(message);

				message = Strings.Command + sql;
				Log.Error(message);

				throw;
			}

			return command;
		}

		/// <summary>
		/// Prepares all necessary class variables by various constructors.
		/// </summary>
		private bool Initialize(bool forceReset = false)
		{
			bool returnValue;

			try
			{
				if (forceReset == true)
				{
					if ((Connection != null) &&
						(Connection.State == ConnectionState.Open))
					{
						Connection.Dispose();
						Connection = null;
					}
				}

				Connection ??= GetConnectionObject(databaseType, connectionText);

				if ((Connection != null) &&
					(Connection.State != ConnectionState.Open))
				{
					Connection.Open();
				}

				returnValue = true;
			}
			catch (Exception exception)
			{
				RollbackTransaction();

				string message = Strings.Exception + exception;
				Log.Error(message);

				throw;
			}

			return returnValue;
		}
	}
}
