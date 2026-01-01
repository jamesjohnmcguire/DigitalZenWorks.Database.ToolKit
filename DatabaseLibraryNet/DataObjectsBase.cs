/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataObjectsBase.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// Namespace includes
/////////////////////////////////////////////////////////////////////////////
namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Data;
	using System.Globalization;
	using System.IO;

	/// <summary>
	/// Base class for database collection classes.
	/// </summary>
	public class DataObjectsBase : IDisposable
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="database">The DataStorage object to use.</param>
		public DataObjectsBase(DataStorage database)
		{
			this.Database = database;
		}

		// Future Use.
		///// <summary>
		///// Initializes a new instance of the <see cref="DataObjectsBase"/>
		///// class.
		///// </summary>
		///// <param name="connectionString">The connection string to
		///// use.</param>
		// public DataObjectsBase(string connectionString)
		// {
		// database = new DataStorage(connectionString);
		// }

		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="dataSource">The data source to use.</param>
		[Obsolete("DataObjectsBase(string) is deprecated, " +
			"please use DataObjectsBase(DatabaseType, string) instead.")]
		public DataObjectsBase(string dataSource)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="databaseType">The database type.</param>
		/// <param name="databaseFilePath">The database file path.</param>
		public DataObjectsBase(
			DatabaseType databaseType, string databaseFilePath)
		{
			this.DatabaseType = databaseType;

			string connectionString = null;

			if (!File.Exists(databaseFilePath))
			{
				databaseFilePath = Environment.GetFolderPath(
					Environment.SpecialFolder.ApplicationData) +
					"\\" + databaseFilePath;
			}

			this.DatabaseFilePath = databaseFilePath;

			switch (databaseType)
			{
				case DatabaseType.SQLite:
					const string connectionBase = "Data Source={0};Version=3;" +
						"DateTimeFormat=InvariantCulture";
					connectionString = string.Format(
						CultureInfo.InvariantCulture,
						connectionBase,
						databaseFilePath);
					break;
				default:
					throw new NotImplementedException();
			}

			Database = new DataStorage(databaseType, connectionString);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="tableName">The table name to use.</param>
		/// <param name="dataSource">The data source to use.</param>
		[Obsolete("DataObjectsBase(string, string) is deprecated, " +
			"please use DataObjectsBase(DatabaseType, string, string) " +
			"instead.")]
		public DataObjectsBase(string tableName, string dataSource)
			: this(dataSource)
		{
			this.TableName = tableName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="databaseType">The database type.</param>
		/// <param name="databaseFilePath">The database file path.</param>
		/// <param name="tableName">The table name to use.</param>
		public DataObjectsBase(
			DatabaseType databaseType,
			string databaseFilePath,
			string tableName)
			: this(databaseType, databaseFilePath)
		{
			this.TableName = tableName;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="tableName">The table name to use.</param>
		/// <param name="databaseType">The database type.</param>
		/// <param name="connectionString">The connection string
		/// to use.</param>
		public DataObjectsBase(
			string tableName,
			DatabaseType databaseType,
			string connectionString)
		{
			this.TableName = tableName;

			Database = new DataStorage(databaseType, connectionString);
		}

		/// <summary>
		/// Gets the database file path.
		/// </summary>
		/// <value>Represents the database file path.</value>
		public string DatabaseFilePath { get; }

		/// <summary>
		/// Gets or sets the core database object.
		/// </summary>
		/// <value>Represents the core database object.</value>
		public DataStorage Database { get; protected set; }

		/// <summary>
		/// Gets the database type.
		/// </summary>
		/// <value>The database type.</value>
		public DatabaseType DatabaseType { get; }

		/// <summary>
		/// Gets or sets the name of the primary database table associated with
		/// this collection.
		/// </summary>
		/// <value>Contains the name of the primary database table associated
		/// with this collection.</value>
		protected string TableName { get; set; }

		/// <summary>
		/// Dispose.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Deletes the record identified by Id.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="idColumn">The name of the primay key column.</param>
		/// <param name="id">The id of item.</param>
		/// <returns>A value indicating success or not.</returns>
		public bool Delete(string tableName, string idColumn, int id)
		{
			bool returnCode;

			string sql = string.Format(
				CultureInfo.InvariantCulture,
				"DELETE FROM {0} WHERE {1} ='{2}'",
				tableName,
				idColumn,
				id);

			returnCode = Database.Delete(sql);

			return returnCode;
		}

		/// <summary>
		/// Returns a DataTable of the table.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <returns>A DataTable object.</returns>
		public DataTable GetAllDataTable(string table)
		{
			DataTable tableList;
			string sql = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT * FROM {0} ORDER BY id",
				table);

			tableList = Database.GetDataTable(sql);

			return tableList;
		}

		/// <summary>
		/// Represents the record identified by the where clause.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <param name="where">The where clause to use.</param>
		/// <returns>A DataRow object.</returns>
		public DataRow GetBy(string table, string where)
		{
			DataRow dataRow;

			string sql = string.Format(
				CultureInfo.InvariantCulture,
				"SELECT * FROM {0} WHERE {1}",
				table,
				where);

			dataRow = Database.GetDataRow(sql);

			return dataRow;
		}

		/// <summary>
		/// Represents the Category record identified by the where clause.
		/// </summary>
		/// <param name="tableName">The table name.</param>
		/// <param name="where">The where clause.</param>
		/// <param name="orderBy">The order by clause.</param>
		/// <param name="limit">The limit clause.</param>
		/// <returns>A DataTable Object.</returns>
		public DataTable GetBy(
			string tableName, string where, string orderBy, string limit)
		{
			string statement =
				"SELECT * FROM " + tableName + " WHERE " + where;

			if (!string.IsNullOrWhiteSpace(orderBy))
			{
				statement += " ORDER BY " + orderBy;
			}

			if (!string.IsNullOrWhiteSpace(limit))
			{
				statement += " LIMIT " + limit;
			}

			DataTable table = Database.GetDataTable(statement);

			return table;
		}

		/// <summary>
		/// Gets the id of the record identified by the where clause.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <param name="name">The name of item to get.</param>
		/// <returns>A DataRow object.</returns>
		public virtual DataRow GetByName(string table, string name)
		{
			return GetBy(table, "`name` = '" + name + "'");
		}

		/// <summary>
		/// Gets the id of the record identified by the where clause.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <param name="name">The name of item to get.</param>
		/// <returns>The id of the record.</returns>
		public virtual int GetIdByName(string table, string name)
		{
			int id = 0;

			DataRow row = GetByName(table, name);

			if (row != null)
			{
				if (!row.IsNull(0))
				{
					id = Convert.ToInt32(row[0], CultureInfo.InvariantCulture);
				}
			}

			return id;
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
				Database?.Close();
				Database = null;
			}
		}
	}
}
