/////////////////////////////////////////////////////////////////////////////
// <copyright file="Schema.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Data;
	using System.Data.Common;
	using System.Data.SQLite;
	using System.Globalization;
	using global::Common.Logging;
	using Microsoft.Data.SqlClient;
	using MySql.Data.MySqlClient;

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
			DataTable schemaTable = null;

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
				if (connection != null)
				{
					connection.Close();
					connection = null;
				}
			}
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

		private DataTable AddForeignKeyConstraints(string tableName, DataTable table)
		{
			try
			{
				string[] tableInformation = [null, null, tableName];

				DataTable foreignKeys = connection.GetSchema(
					"ForeignKeys", tableInformation);

				foreach (DataRow row in foreignKeys.Rows)
				{
					DataRow newRow = table.NewRow();

					newRow["ConstraintType"] = "FOREIGN KEY";
					newRow["ConstraintName"] = row["CONSTRAINT_NAME"];
					newRow["TableName"] = row["TABLE_NAME"];
					newRow["ColumnName"] = row["COLUMN_NAME"];
					newRow["ReferencedTable"] = row["REFERENCED_TABLE_NAME"];
					newRow["ReferencedColumn"] = row["REFERENCED_COLUMN_NAME"];

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
	}
}
