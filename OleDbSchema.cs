/////////////////////////////////////////////////////////////////////////////
// <copyright file="OleDbSchema.cs" company="James John McGuire">
// Copyright © 2006 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Data;
using System.Data.OleDb;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>OleDbSchema.</c>
	/// <summary>
	/// Represents an OleDbSchema object.
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class OleDbSchema : IDisposable
	{
		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents an OleDb open connection to a data source.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private OleDbConnection oleDbConnection;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="OleDbSchema"/> class.
		/// </summary>
		/// <param name="databaseFile"></param>
		/////////////////////////////////////////////////////////////////////
		public OleDbSchema(string databaseFile)
		{
			string connectionString =
				DatabaseUtilities.MakePrivilegedConnectString(databaseFile);

			oleDbConnection = new OleDbConnection(connectionString);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets all table names from the connected database.
		/// </summary>
		/// <returns>DataTable.</returns>
		/// <value>
		/// All table names from the connected database.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public DataTable TableNames
		{
			get
			{
				oleDbConnection.Open();

				DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
					System.Data.OleDb.OleDbSchemaGuid.Tables,
					new object[] { null, null, null, "TABLE" });

				oleDbConnection.Close();

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

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the constraints from the given table.
		/// </summary>
		/// <returns>DataTable.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetConstraints(string tableName)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Constraint_Column_Usage,
				new object[] { null, null, tableName, null, null, null });

			oleDbConnection.Close();

			return schemaTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the foreign keys from the given table.
		/// </summary>
		/// <returns>DataTable.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetForeignKeys(string tableName)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Foreign_Keys,
				new object[] { null, null, tableName });

			oleDbConnection.Close();

			return schemaTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the primary keys from the given table.
		/// </summary>
		/// <returns>DataTable.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetPrimaryKeys(string tableName)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Primary_Keys,
				new object[] { null, null, tableName });

			oleDbConnection.Close();

			return schemaTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the column names from the given table.
		/// </summary>
		/// <returns>DataTable.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetTableColumns(string tableName)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Columns,
				new object[] { null, null, tableName });

			oleDbConnection.Close();

			return schemaTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the constraints from the given table.
		/// </summary>
		/// <returns>DataTable.</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetTableConstraints(string tableName)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Table_Constraints,
				new object[] { null, null, null, null, null, tableName });

			oleDbConnection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Dispose.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (null != oleDbConnection)
				{
					oleDbConnection.Close();
					oleDbConnection = null;
				}
			}
		}
	}
}
