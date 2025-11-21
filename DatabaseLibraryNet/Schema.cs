/////////////////////////////////////////////////////////////////////////////
// <copyright file="Schema.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using global::Common.Logging;
	using Microsoft.Data.SqlClient;
	using MySql.Data.MySqlClient;
	using System;
	using System.Configuration.Provider;
	using System.Data;
	using System.Data.Common;
	using System.Data.OleDb;
	using System.Data.SQLite;
	using System.Globalization;

	/// Class <c>Schema.</c>
	/// <summary>
	/// Represents a database schema.
	/// </summary>
	public class Schema : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Represents the database type.
		/// </summary>
		private readonly DatabaseType databaseType = DatabaseType.Unknown;

		/// <summary>
		/// Represents a connection to a data source.
		/// </summary>
		private DbConnection connection;

		/// <summary>
		/// Initializes a new instance of the <see cref="Schema"/> class.
		/// </summary>
		/// <param name="databaseType">The database type.</param>
		/// <param name="databaseFile">The database file to use.</param>
		public Schema(DatabaseType databaseType, string databaseFile)
		{
			string baseFormat = "Data Source={0}";

			string connectionString = string.Format(
				CultureInfo.InvariantCulture,
				baseFormat,
				databaseFile);

			connection = GetConnection(databaseType, connectionString);
		}

		/// <summary>
		/// Gets all table names from the connected database.
		/// </summary>
		/// <returns>DataTable.</returns>
		/// <value>
		/// All table names from the connected database.
		/// </value>
		public DataTable TableNames
		{
			get
			{
				connection.Open();

				DataTable schemaTable = connection.GetSchema("Tables");

				connection.Close();

				return schemaTable;
			}
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
		/// Gets the constraints from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetConstraints(string tableName)
		{
			DataTable constraints = GetBaseConstraints();

			constraints = AddForeignKeyConstraints(
				tableName, constraints);

			constraints = AddIndexConstraints(
				tableName, constraints);

			return constraints;
		}

		/// <summary>
		/// Gets the primary keys from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetPrimaryKeys(string tableName)
		{
			connection.Open();

			string[] tableInformation = [null, null, tableName];

			DataTable schemaTable = connection.GetSchema(
				"PrimaryKeys", tableInformation);

			connection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Gets the column names from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetTableColumns(string tableName)
		{
			connection.Open();

			object[] testTable = [null, null, tableName];

			DataTable schemaTable = connection.GetSchema("Columns");

			connection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Dispose.
		/// </summary>
		/// <param name="disposing">Indicates whether the object is
		/// currently disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				connection?.Close();
				connection = null;
			}
		}

		private static DataTable GetBaseConstraints()
		{
			DataTable table = new ();

			Type stringType = typeof(string);

			table.Columns.Add("ConstraintType", stringType);
			table.Columns.Add("ConstraintName", stringType);
			table.Columns.Add("TableName", stringType);
			table.Columns.Add("ColumnName", stringType);
			table.Columns.Add("ReferencedTable", stringType);
			table.Columns.Add("ReferencedColumn", stringType);

			return table;
		}

		private static string GetConstraintQueryMySql(string tableName)
		{
			string fields = "CONSTRAINT_TYPE, CONSTRAINT_NAME, TABLE_NAME, " +
				"COLUMN_NAME, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME";

			string from = "INFORMATION_SCHEMA.KEY_COLUMN_USAGE";

			string union = "UNION SELECT 'PRIMARY KEY' as CONSTRAINT_TYPE, " +
				"CONSTRAINT_NAME, TABLE_NAME, COLUMN_NAME, " +
				"NULL as REFERENCED_TABLE_NAME, " +
				"NULL as REFERENCED_COLUMN_NAME";

			string where = $"WHERE TABLE_NAME = '{tableName}' " +
				"AND CONSTRAINT_NAME != 'PRIMARY'";

			string query = $"SELECT {fields} FROM {from} " +
				$"{union} FROM {from} {where}";

			return query;
		}

		private static string GetConstraintQueryOracle(string tableName)
		{
			string fields = @"SELECT constraints.CONSTRAINT_TYPE,
				constraints.CONSTRAINT_NAME,
				constraints.TABLE_NAME,
				constraintColumns.COLUMN_NAME,
				constraints.R_CONSTRAINT_NAME,
				referentialConstraints.TABLE_NAME as REFERENCED_TABLE_NAME,
				referentialConstraintColumns.COLUMN_NAME as
				REFERENCED_COLUMN_NAME";

			string from = "ALL_CONSTRAINTS constraints";

			string joins =
				"LEFT JOIN ALL_CONS_COLUMNS constraintColumns ON " +
				"constraints.CONSTRAINT_NAME = " +
				"constraintColumns.CONSTRAINT_NAME " +
				"LEFT JOIN ALL_CONSTRAINTS referentialConstraints ON " +
				"constraints.R_CONSTRAINT_NAME = " +
				"referentialConstraints.CONSTRAINT_NAME " +
				"LEFT JOIN ALL_CONS_COLUMNS referentialConstraintColumns ON " +
				"referentialConstraints.CONSTRAINT_NAME = " +
				"referentialConstraintColumns.CONSTRAINT_NAME";

			string where =
				$"WHERE constraints.TABLE_NAME = UPPER('{tableName}')";

			string query = $"SELECT {fields} FROM {from} {joins} {where}";

			return query;
		}

		private static string GetConstraintQueryPostgresSql(string tableName)
		{
			string fields = @"SELECT tableConstraints.constraint_type,
				tableConstraints.constraint_name,
				tableConstraints.table_name,
				constraintColumnUsage.column_name,
				constraintColumnUsageNext.table_name as referenced_table_name,
				constraintColumnUsageNext.column_name as
				referenced_column_name";

			string from =
				"information_schema.table_constraints tableConstraints";

			string joins =
				"LEFT JOIN information_schema.constraint_column_usage " +
				"constraintColumnUsage ON " +
				"tableConstraints.constraint_name= " +
				"constraintColumnUsage.constraint_name" +
				"LEFT JOIN information_schema.referential_constraints " +
				"referentialConstraints ON " +
				"tableConstraints.constraint_name = " +
				"referentialConstraints.constraint_name" +
				"LEFT JOIN information_schema.constraint_column_usage " +
				"constraintColumnUsageNext ON " +
				"referentialConstraints.unique_constraint_name = " +
				"constraintColumnUsageNext.constraint_name";

			string where =
				$"WHERE tableConstraints.table_name = '{tableName}'";

			string query = $"SELECT {fields} FROM {from} {joins} {where}";

			return query;
		}

		private string GetConstraintQuerySqlite(string tableName)
		{
			string query = $"PRAGMA foreign_key_list('{tableName}')";

			return query;
		}

		private static string GetConstraintQuerySqlServer(string tableName)
		{
			string fields = @"SELECT tableConstraints.CONSTRAINT_TYPE,
				tableConstraints.CONSTRAINT_NAME,
				tableConstraints.TABLE_NAME,
				constraintColumnUsage.COLUMN_NAME,
				rc.UNIQUE_CONSTRAINT_NAME,
				ccu2.TABLE_NAME as REFERENCED_TABLE_NAME,
				ccu2.COLUMN_NAME as REFERENCED_COLUMN_NAME";

			string from =
				"INFORMATION_SCHEMA.TABLE_CONSTRAINTS tableConstraints";

			string joins =
				"LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE " +
				"constraintColumnUsage ON " +
				"tableConstraints.CONSTRAINT_NAME = " +
				"constraintColumnUsage.CONSTRAINT_NAME " +
				"LEFT JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS " +
				"referentialConstraints ON " +
				"tableConstraints.CONSTRAINT_NAME = " +
				"referentialConstraints.CONSTRAINT_NAME " +
				"LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE " +
				"constraintColumnUsageNext ON " +
				"referentialConstraints.UNIQUE_CONSTRAINT_NAME = " +
				"constraintColumnUsageNext.CONSTRAINT_NAME";

			string where =
				$"WHERE tableConstraints.TABLE_NAME = '{tableName}'";

			string query = $"SELECT {fields} FROM {from} {joins} {where}";

			return query;
		}

		private static DataRow GetForeignKeyConstaintsRow(
			DataTable table, DataRow row)
		{
			DataRow newRow = table.NewRow();

			newRow["ConstraintType"] = "FOREIGN KEY";
			newRow["ConstraintName"] = row["CONSTRAINT_NAME"];
			newRow["TableName"] = row["TABLE_NAME"];
			newRow["ColumnName"] = row["COLUMN_NAME"];
			newRow["ReferencedTable"] = row["REFERENCED_TABLE_NAME"];
			newRow["ReferencedColumn"] = row["REFERENCED_COLUMN_NAME"];

			return newRow;
		}

		private static DataRow GetIndexConstaintsRow(
			DataTable table, DataRow row)
		{
			DataRow newRow = table.NewRow();

			newRow["ConstraintType"] = "PRIMARY KEY";
			newRow["ConstraintName"] = row["INDEX_NAME"];
			newRow["TableName"] = row["TABLE_NAME"];
			newRow["ColumnName"] = row["COLUMN_NAME"];

			return newRow;
		}

		private DataTable AddForeignKeyConstraints(
			string tableName, DataTable table)
		{
			try
			{
				string[] tableInformation = [null, null, tableName];

				DataTable foreignKeys = connection.GetSchema(
					"ForeignKeys", tableInformation);

				foreach (DataRow row in foreignKeys.Rows)
				{
					DataRow newRow = GetForeignKeyConstaintsRow(table, row);

					table.Rows.Add(newRow);
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is NotSupportedException ||
				exception is InvalidOperationException)
			{
				// Some providers might not support ForeignKeys schema
				Log.Error(exception.ToString());
			}
			catch (Exception exception)
			{
				Log.Error(exception.ToString());

				throw;
			}

			return table;
		}

		private DataTable AddIndexConstraints(
			string tableName, DataTable table)
		{
			try
			{
				string[] tableInformation = [null, null, tableName];
				DataTable primaryKeys =
					connection.GetSchema("IndexColumns", tableInformation);

				foreach (DataRow row in primaryKeys.Rows)
				{
					bool exists = row["PRIMARY_KEY"] != DBNull.Value;
					bool isPrimaryKey = (bool)row["PRIMARY_KEY"];

					if (exists == true && isPrimaryKey == true)
					{
						DataRow newRow = GetIndexConstaintsRow(table, row);

						table.Rows.Add(newRow);
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is NotSupportedException ||
				exception is InvalidOperationException)
			{
				// Some providers might not support ForeignKeys schema
				Log.Error(exception.ToString());
			}
			catch (Exception exception)
			{
				Log.Error(exception.ToString());

				throw;
			}

			return table;
		}

		private DbConnection GetConnection(
			DatabaseType databaseType, string connectionText)
		{
			switch (databaseType)
			{
				case DatabaseType.MySql:
					MySqlConnection mySqlConnection = new (connectionText);
					connection = mySqlConnection;
					break;
				case DatabaseType.SQLite:
					SQLiteConnection sqliteConnection = new (connectionText);
					connection = sqliteConnection;
					break;
				case DatabaseType.SqlServer:
					SqlConnection sqlConnection = new (connectionText);
					connection = sqlConnection;
					break;
				case DatabaseType.Unknown:
					break;
				case DatabaseType.Oracle:
					break;
				default:
					break;
			}

			return connection;
		}

		private DataTable GetConstraintsByProvider(string tableName)
		{
			string query = GetConstraintQueryByProvider(tableName);

			using DbCommand command = connection.CreateCommand();
			command.CommandText = query;
			using var adapter =
				DbProviderFactories.GetFactory(connection).CreateDataAdapter();
			adapter.SelectCommand = command;

			DataTable constraints = new ();
			adapter.Fill(constraints);

			return constraints;
		}

		private string GetConstraintQueryByProvider(string tableName)
		{
			string query = databaseType switch
			{
				DatabaseType.MySql => GetConstraintQueryMySql(tableName),
				DatabaseType.Oracle => GetConstraintQueryOracle(tableName),
				DatabaseType.PostgresSql =>
					GetConstraintQueryPostgresSql(tableName),
				DatabaseType.SQLite => GetConstraintQuerySqlite(tableName),
				DatabaseType.SqlServer =>
					GetConstraintQuerySqlServer(tableName),

				_ => throw new NotSupportedException(
					$"Provider not supported for constraint queries")
			};

			return query;
		}
	}
}
