/////////////////////////////////////////////////////////////////////////////
// $Id: $
//
// Copyright © 2006 - 2016 by James John McGuire (DigitalZenWorks)
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////

/////////////////////////////////////////////////////////////////////////////
// Namespace includes
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Data;
using System.Globalization;
using System.IO;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Base class for database collection classes
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class DataObjectsBase: IDisposable
	{
		private DataStorage database = null;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents a provider type for a connection string
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private const string provider = "Microsoft.ACE.OLEDB.12.0";

		private string tableName = null;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents the core database object
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[CLSCompliantAttribute(false)]
		protected DataStorage Database
		{
			get { return database; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Contains the name of the primary database table associated with
		/// this collection
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		protected string TableName
		{
			get { return tableName; }
			set { tableName = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents a base collection of data objects
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public DataObjectsBase(string databaseFileName)
		{
			string dataSource = databaseFileName;

			if (!File.Exists(databaseFileName))
			{
				dataSource = AppDomain.CurrentDomain.BaseDirectory +
					databaseFileName;
			}

			database = new DataStorage(provider, dataSource);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Dispose
		/// </summary>
		/// <param name="disposing"></param>
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

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Dispose
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Deletes the record identified by Id
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public bool Delete(string table, int id)
		{
			bool returnCode = false;

			string sql = string.Format(CultureInfo.InvariantCulture,
				@"DELETE FROM {0} WHERE id ='{1}'", table, id);

			returnCode = database.Delete(sql);

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns a DataTable of the table
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetAllDataTable(string table)
		{
			DataTable tableList = null;
			string sql = string.Format(CultureInfo.InvariantCulture,
				@"SELECT * FROM {0} ORDER BY id", table);

			database.GetDataTable(sql, out tableList);

			return tableList;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents the record identified by the where clause
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public DataRow GetBy(string table, string where)
		{
			string sql = string.Format(CultureInfo.InvariantCulture,
				@"SELECT * FROM {0} WHERE {1}", table, where);

			DataRow	dataRow	= null;

			database.GetDataRow(sql, out dataRow);

			return dataRow;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the id of the record identified by the where clause
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public int GetIdByName(string tableName, string name)
		{
			int id = 0;

			DataRow row = GetBy(tableName, "`name` = '" + name + "'");

			if (null != row)
			{
				if (DBNull.Value != row[0])
				{
					id = Convert.ToInt32(row[0]);
				}
			}

			return id;
		}

	} // End Class
} // end Namespace