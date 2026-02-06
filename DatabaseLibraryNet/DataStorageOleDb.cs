/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataStorageOleDb.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.Common;
	using System.Data.OleDb;
	using System.Globalization;
	using System.Runtime.Versioning;
	using global::Common.Logging;

	/// Class <c>DataStorageOleDb.</c>
	/// <summary>
	/// Class for OleDb database access independent of the underlying
	/// transport.
	/// </summary>
	/// <remarks>
	/// Initializes a new instance of the <see cref="DataStorageOleDb"/>
	/// class.
	/// </remarks>
	/// <param name="connectionString">The connection string.</param>
#if NET5_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public class DataStorageOleDb(string connectionString)
		: DataStorage(DatabaseType.OleDb, connectionString)
	{
		/// <summary>
		/// Diagnostics object.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Ole Database Connection Object.
		/// </summary>
		private OleDbConnection oleDbConnection;

		/// <summary>
		/// Gets the database command object.
		/// </summary>
		public override DbCommand Command
		{
			get
			{
				DbCommand command = new OleDbCommand();

				return command;
			}
		}

		/// <summary>
		/// Gets the data adapter object.
		/// </summary>
		public override DbDataAdapter DataAdapter
		{
			get
			{
				DbDataAdapter dataAdapter = new OleDbDataAdapter();

				return dataAdapter;
			}
		}

		/// <summary>
		/// Adds parameters to the command.
		/// </summary>
		/// <param name="command">The command to use.</param>
		/// <param name="values">The list of values.</param>
		/// <returns>An updated collection of values.</returns>
		public static OleDbParameterCollection AddParameters(
			OleDbCommand command, IDictionary<string, object> values)
		{
#if NET6_0_OR_GREATER
			ArgumentNullException.ThrowIfNull(command);
#else
			if (command == null)
			{
				throw new ArgumentNullException(nameof(command));
			}
#endif

			OleDbParameterCollection parameters = command.Parameters;

			if (values != null)
			{
				foreach (KeyValuePair<string, object> valuePair in values)
				{
					string name = "@" + valuePair.Key;
					OleDbParameter parameter;

					if (valuePair.Value == null)
					{
						parameter = parameters.AddWithValue(name, DBNull.Value);
					}
					else
					{
						parameter =
							parameters.AddWithValue(name, valuePair.Value);
					}

					if (parameter == null)
					{
						Log.Warn("Parameters.AddWithValue returns null");
					}
				}
			}

			return parameters;
		}

		/// <summary>
		/// Generates a database connection string for the OleDb database
		/// type and data source.
		/// </summary>
		/// <remarks>The returned connection string uses default credentials
		/// and settings.</remarks>
		/// <param name="dataSource">The data source identifier, such as a
		/// server name, file path, or connection endpoint, used to construct
		/// the connection string. Cannot be null.</param>
		/// <returns>A connection string formatted for the OleDb database
		/// type and data source.</returns>
		public static string GetConnectionString(string dataSource)
		{
			string connectionString =
				OleDbHelper.BuildConnectionString(dataSource);

			return connectionString;
		}

		/// <summary>
		/// Dispose.
		/// </summary>
		/// <param name="disposing">Indicates whether it is
		/// currently disposing.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				if (oleDbConnection != null)
				{
					oleDbConnection.Close();
					oleDbConnection.Dispose();
					oleDbConnection = null;
				}
			}
		}

		/// <summary>
		/// Gets the database connection object.
		/// </summary>
		/// <param name="databaseType">The database type.</param>
		/// <param name="connectionText">The connection text.</param>
		/// <returns>A DbConnection object or null.</returns>
		protected override DbConnection GetConnectionObject(
			DatabaseType databaseType, string connectionText)
		{
			// Two statements help in debugging problems
			oleDbConnection = new OleDbConnection(connectionText);
			DbConnection connection = oleDbConnection;

			return connection;
		}

		/// <summary>
		/// Gets the table schema information for the associated database.
		/// </summary>
		/// <returns>The table of tables.</returns>
		protected override DataTable GetTables()
		{
			DataTable tables = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Tables,
				[null, null, null, "TABLE"]);

			return tables;
		}
	}
}
