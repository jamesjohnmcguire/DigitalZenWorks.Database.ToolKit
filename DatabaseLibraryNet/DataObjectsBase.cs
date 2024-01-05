/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataObjectsBase.cs" company="James John McGuire">
// Copyright © 2006 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// Namespace includes
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Data;
using System.Globalization;
using System.IO;

namespace DigitalZenWorks.Database.ToolKit
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Base class for database collection classes.
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class DataObjectsBase
		: IDisposable
	{
		private DataStorage database;
		private string tableName;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="database">The DataStorage object to use.</param>
		/////////////////////////////////////////////////////////////////////
		public DataObjectsBase(DataStorage database)
		{
			this.database = database;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="dataSource">The data source to use.</param>
		/////////////////////////////////////////////////////////////////////
		public DataObjectsBase(string dataSource)
		{
			if (!File.Exists(dataSource))
			{
				dataSource = Environment.GetFolderPath(
					Environment.SpecialFolder.ApplicationData) +
					"\\" + dataSource;
			}

			string connectionString = string.Format(
				CultureInfo.InvariantCulture,
				"provider={0}; Data Source={1}",
				"Microsoft.ACE.OLEDB.12.0",
				dataSource);

			database = new DataStorage(DatabaseType.OleDb, connectionString);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="tableName">The table name to use.</param>
		/// <param name="dataSource">The data source to use.</param>
		/////////////////////////////////////////////////////////////////////
		public DataObjectsBase(string tableName, string dataSource)
			: this(dataSource)
		{
			this.tableName = tableName;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="DataObjectsBase"/>
		/// class.
		/// </summary>
		/// <param name="tableName">The table name to use.</param>
		/// <param name="databaseType">The database type.</param>
		/// <param name="connectionString">The connection string
		/// to use.</param>
		/////////////////////////////////////////////////////////////////////
		public DataObjectsBase(
			string tableName,
			DatabaseType databaseType,
			string connectionString)
		{
			this.tableName = tableName;

			database = new DataStorage(databaseType, connectionString);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the core database object.
		/// </summary>
		/// <value>Represents the core database object.</value>
		/////////////////////////////////////////////////////////////////////
		[CLSCompliantAttribute(false)]
		protected DataStorage Database
		{
			get { return database; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets the name of the primary database table associated with
		/// this collection.
		/// </summary>
		/// <value>Contains the name of the primary database table associated
		/// with this collection.</value>
		/////////////////////////////////////////////////////////////////////
		protected string TableName
		{
			get { return tableName; }
			set { tableName = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Dispose.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Deletes the record identified by Id.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <param name="id">The id of item.</param>
		/// <returns>A value indicating success or not.</returns>
		/////////////////////////////////////////////////////////////////////
		public bool Delete(string table, int id)
		{
			bool returnCode;

			string sql = string.Format(
				CultureInfo.InvariantCulture,
				@"DELETE FROM {0} WHERE id ='{1}'",
				table,
				id);

			returnCode = database.Delete(sql);

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a DataTable of the table.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <returns>A DataTable object.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetAllDataTable(string table)
		{
			DataTable tableList;
			string sql = string.Format(
				CultureInfo.InvariantCulture,
				@"SELECT * FROM {0} ORDER BY id",
				table);

			tableList = database.GetDataTable(sql);

			return tableList;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents the record identified by the where clause.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <param name="where">The where clause to use.</param>
		/// <returns>A DataRow object.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataRow GetBy(string table, string where)
		{
			DataRow dataRow;

			string sql = string.Format(
				CultureInfo.InvariantCulture,
				@"SELECT * FROM {0} WHERE {1}",
				table,
				where);

			dataRow = database.GetDataRow(sql);

			return dataRow;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the id of the record identified by the where clause.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <param name="name">The name of item to get.</param>
		/// <returns>A DataRow object.</returns>
		/////////////////////////////////////////////////////////////////////
		public virtual DataRow GetByName(string table, string name)
		{
			return GetBy(table, "`name` = '" + name + "'");
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the id of the record identified by the where clause.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <param name="name">The name of item to get.</param>
		/// <returns>The id of the record.</returns>
		/////////////////////////////////////////////////////////////////////
		public virtual int GetIdByName(string table, string name)
		{
			int id = 0;

			DataRow row = GetByName(table, name);

			if (null != row)
			{
				if (!row.IsNull(0))
				{
					id = Convert.ToInt32(row[0], CultureInfo.InvariantCulture);
				}
			}

			return id;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Dispose.
		/// </summary>
		/// <param name="disposing">Indicates whether it is
		/// currently disposing.</param>
		/////////////////////////////////////////////////////////////////////
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != database)
				{
					database.Close();
					database = null;
				}
			}
		}
	} // End Class
} // end Namespace
