/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseConnection.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Data.SQLite;
	using Dapper;
	using Microsoft.Data.SqlClient;
	using MySql.Data.MySqlClient;

	/// <summary>
	/// The database connection class.
	/// </summary>
	public class DatabaseConnection : IDisposable
	{
		private readonly string connectionString;
		private IDbConnection? connection;

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseConnection"/>
		/// class.
		/// </summary>
		/// <param name="connectionString">The connection string
		/// to use.</param>
		public DatabaseConnection(string connectionString)
			: this(DatabaseType.SQLite, connectionString)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseConnection"/>
		/// class.
		/// </summary>
		/// <param name="databaseType">The database type.</param>
		/// <param name="connectionString">The connection string
		/// to use.</param>
		public DatabaseConnection(
			DatabaseType databaseType, string connectionString)
		{
			this.connectionString = connectionString;
			connection = GetConnection(databaseType, connectionString);
		}

		/// <summary>
		/// Begin transaction.
		/// </summary>
		/// <returns>The transaction object.</returns>
		public IDbTransaction BeginTransaction()
		{
			IDbTransaction transaction = connection!.BeginTransaction();

			return transaction;
		}

		/// <summary>
		/// The dispose method.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Execute a SQL statement.
		/// </summary>
		/// <param name="statement">The SQL statement to execute.</param>
		/// <param name="item">The item.</param>
		/// <returns>A result indicator.</returns>
		public int Execute(string statement, object? item)
		{
			int result = connection!.Execute(statement, item);

			return result;
		}

		/// <summary>
		/// Execute a SQL statement.
		/// </summary>
		/// <param name="statement">The SQL statement to execute.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="transaction">The database transaction.</param>
		/// <returns>A result indicator.</returns>
		public int Execute(
			string statement,
			object? parameter,
			IDbTransaction transaction)
		{
			int result = connection!.Execute(statement, parameter, transaction);

			return result;
		}

		/// <summary>
		/// Query SQL statment.
		/// </summary>
		/// <typeparam name="T">The type of object to enumerate.</typeparam>
		/// <param name="statement">The SQL statement to execute.</param>
		/// <returns>A list of items.</returns>
		public IEnumerable<T> Query<T>(
			string statement)
		{
			IEnumerable<T> list = connection!.Query<T>(statement);

			return list;
		}

		/// <summary>
		/// Query for single or default from SQL statment.
		/// </summary>
		/// <typeparam name="T">The type of object to enumerate.</typeparam>
		/// <param name="statement">The SQL statement to execute.</param>
		/// <param name="parameter">The parameter.</param>
		/// <returns>A list of items.</returns>
		public T? QuerySingleOrDefault<T>(
			string statement,
			object? parameter)
		{
			T? item =
				connection!.QuerySingleOrDefault<T>(statement, parameter);

			return item;
		}

		/// <summary>
		/// Query for single from SQL statment.
		/// </summary>
		/// <typeparam name="T">The type of object to enumerate.</typeparam>
		/// <param name="statement">The SQL statement to execute.</param>
		/// <param name="parameter">The parameter.</param>
		/// <param name="transaction">The database transaction.</param>
		/// <returns>An item.</returns>
		public T QuerySingle<T>(
			string statement,
			object? parameter,
			IDbTransaction transaction)
		{
			T item =
				connection!.QuerySingle<T>(statement, parameter, transaction);

			return item;
		}

		/// <summary>
		/// The dispose method.
		/// </summary>
		/// <param name="disposing">Indicates whether it is
		/// currently disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (connection != null)
				{
					connection.Dispose();
					connection = null;
				}
			}
		}

		private IDbConnection? GetConnection(
			DatabaseType databaseType, string connectionString)
		{
			switch (databaseType)
			{
				case DatabaseType.MySql:
					MySqlConnection mySqlConnection = new(connectionString);
					connection = mySqlConnection;
					break;
				case DatabaseType.SQLite:
					SQLiteConnection sqliteConnection = new(connectionString);
					connection = sqliteConnection;
					break;
				case DatabaseType.SqlServer:
					connection = new SqlConnection(connectionString);
					break;
				case DatabaseType.Oracle:
				case DatabaseType.Unknown:
				default:
					break;
			}

			return connection;
		}
	}
}
