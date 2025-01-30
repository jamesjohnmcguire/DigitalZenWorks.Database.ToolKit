/////////////////////////////////////////////////////////////////////////////
// <copyright file="IDataStorage.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// Database collection interface.
	/// </summary>
	public interface IDataStorage : IDisposable
	{
		/// <summary>
		/// Gets get the table schema information for the associated database.
		/// </summary>
		/// <value>
		/// Get the table schema information for the associated database.
		/// </value>
		public DataTable SchemaTable { get; }

		/// <summary>
		/// Gets or sets or set the time out value.
		/// </summary>
		/// <value>
		/// The time out value.
		/// </value>
		public int TimeOut { get; set; }

		/// <summary>
		/// Gets the database connection object.
		/// </summary>
		/// <value>
		/// The database connection object.
		/// </value>
		public DbConnection Connection { get; }

		/// <summary>
		/// Converts a data table to a list.
		/// </summary>
		/// <typeparam name="TItem">The type of item.</typeparam>
		/// <param name="dataTable">The data table to convert.</param>
		/// <returns>Returns a list of items from the data table.</returns>
		Collection<TItem> ConvertDataTable<TItem>(
			DataTable dataTable);

		/// <summary>
		/// Closes the database connection and object.
		/// </summary>
		void Close();

		/// <summary>
		/// Shut down the database.
		/// </summary>
		public void Shutdown();

		/// <summary>
		/// This opens a connection and begins the transaction.
		/// </summary>
		public void BeginTransaction();

		/// <summary>
		/// This closes the transaction.
		/// </summary>
		public void CloseTransaction();

		/// <summary>
		/// This commits the transaction.
		/// </summary>
		public void CommitTransaction();

		/// <summary>
		/// This rolls back the transaction.
		/// </summary>
		public void RollbackTransaction();

		/// <summary>
		/// Checks to see if the database can return a valid query. Helper
		/// function for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise.</returns>
		public bool CanQuery();

		/// <summary>
		/// Performs an SQL DELETE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>Success / Failure.</returns>
		public bool Delete(string sql);

		/// <summary>
		/// Prepares and executes a Non-Query DB Command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>A value indicating success or not.</returns>
		public bool ExecuteNonQuery(string sql);

		/// <summary>
		/// Prepares and executes a Non-Query DB Command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>A value indicating success or not.</returns>
		public bool ExecuteNonQuery(
			string sql, IDictionary<string, object> values);

		/// <summary>
		/// Execture a data reader.
		/// </summary>
		/// <remarks>Caller is responsible for disposing returned
		/// object.</remarks>
		/// <param name="statement">the SQL statement to execute.</param>
		/// <returns>A data reader.</returns>
		public DbDataReader ExecuteReader(string statement);

		/// <summary>
		/// Gets a single field from a row of data.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>the field object.</returns>
		public object GetDataField(string sql);

		/// <summary>
		/// Gets a single row of data.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>DataRow.</returns>
		public DataRow GetDataRow(string sql);

		/// <summary>
		/// Gets a single row of data.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values of fields to get.</param>
		/// <returns>DataRow, null on failure.</returns>
		public DataRow GetDataRow(
			string sql, IDictionary<string, object> values);

		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>DataSet or null on failure.</returns>
		public DataSet GetDataSet(string sql);

		/// <summary>
		/// Gets a DataSet based on the given query.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>DataSet or null on failure.</returns>
		public DataSet GetDataSet(
			string sql, IDictionary<string, object> values);

		/// <summary>
		/// GetDataTable.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>number of records retrieved.</returns>
		public DataTable GetDataTable(string sql);

		/// <summary>
		/// GetDataTable.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>DataTable or null on failure.</returns>
		public DataTable GetDataTable(
			string sql, IDictionary<string, object> values);

		/// <summary>
		/// Get the last insert id.
		/// </summary>
		/// <returns>The last insert id.</returns>
		/// <exception cref="NotImplementedException">Throws a not implemented
		/// exception for databases that do not have this support.</exception>
		public int GetLastInsertId();

		/// <summary>
		/// Performs an Sql UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>object item.</returns>
		public int Insert(string sql);

		/// <summary>
		/// Performs an Sql UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>object item.</returns>
		public int Insert(string sql, IDictionary<string, object> values);

		/// <summary>
		/// Checks to see if the database is open and connected. Helper function
		/// for unit tests.
		/// </summary>
		/// <returns>true if connection is open, false otherwise.</returns>
		public bool IsConnected();

		/// <summary>
		/// Opens the database.
		/// </summary>
		/// <returns>A values indicating success or not.</returns>
		public bool Open();

		/// <summary>
		/// Performs an Sql UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <returns>A value indicating success or not.</returns>
		public bool Update(string sql);

		/// <summary>
		/// Performs an SQL UPDATE command.
		/// </summary>
		/// <param name="sql">The sql statement to execute.</param>
		/// <param name="values">The values to use in the query.</param>
		/// <returns>A value indicating success or not.</returns>
		public bool Update(string sql, IDictionary<string, object> values);
	}
}
