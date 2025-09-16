/////////////////////////////////////////////////////////////////////////////
// <copyright file="Schema.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using Microsoft.Data.SqlClient;
	using MySql.Data.MySqlClient;
	using System;
	using System.Data;
	using System.Data.Common;
	using System.Data.OleDb;
	using System.Data.SQLite;
	using System.Globalization;

	/// Class <c>Schema.</c>
	/// <summary>
	/// Represents a database schema.
	/// </summary>
	public class Schema
	{
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

		private DbConnection GetConnection(
			DatabaseType databaseType, string connectionText)
		{
			switch (databaseType)
			{
				case DatabaseType.MySql:
					MySqlConnection mySqlConnection =
						new MySqlConnection(connectionText);
					connection = mySqlConnection;
					break;
				case DatabaseType.SQLite:
					SQLiteConnection sqliteConnection =
						new SQLiteConnection(connectionText);
					connection = sqliteConnection;
					break;
				case DatabaseType.SqlServer:
					SqlConnection Connection =
						new SqlConnection(connectionText);
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
	}
}
