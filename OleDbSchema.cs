/////////////////////////////////////////////////////////////////////////////
// $Id: $
//
// Copyright (c) 2006-2014 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Data;
using System.Data.OleDb;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>OleDbSchema</c>
	/// <summary>
	/// Represents an OleDbSchema object
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class OleDbSchema
	{
		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents an OleDb open connection to a data source.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private OleDbConnection oleDbConnection;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="databaseFile"></param>
		/////////////////////////////////////////////////////////////////////
		public OleDbSchema(string databaseFile)
		{
			string connectionString =
				DatabaseUtilities.MakePriviledgedConnectString(databaseFile);

			oleDbConnection = new OleDbConnection(connectionString);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the constraints from the given table
		/// </summary>
		/// <returns>DataTable</returns>
		/////////////////////////////////////////////////////////////////////
		private DataTable GetConstraints(string tablename)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Constraint_Column_Usage,
				new Object[] { null, null, tablename, null, null, null });

			oleDbConnection.Close();

			return schemaTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the foreign keys from the given table
		/// </summary>
		/// <returns>DataTable</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetForeignKeys(string tablename)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Foreign_Keys,
				new Object[] { null, null, tablename });

			oleDbConnection.Close();

			return schemaTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the primary keys from the given table
		/// </summary>
		/// <returns>DataTable</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetPrimaryKeys(string tablename)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Primary_Keys,
				new Object[] { null, null, tablename });

			oleDbConnection.Close();

			return schemaTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the column names from the given table
		/// </summary>
		/// <returns>DataTable</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetTableColumns(string tablename)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Columns,
				new Object[] { null, null, tablename });

			oleDbConnection.Close();

			return schemaTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the constraints from the given table
		/// </summary>
		/// <returns>DataTable</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetTableConstraints(string tablename)
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Table_Constraints,
				new Object[] { null, null, null, null, null, tablename });

			oleDbConnection.Close();

			return schemaTable;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets all table names from the connected database
		/// </summary>
		/// <returns>DataTable</returns>
		/////////////////////////////////////////////////////////////////////
		public DataTable GetTableNames()
		{
			oleDbConnection.Open();

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				System.Data.OleDb.OleDbSchemaGuid.Tables,
				new Object[] { null, null, null, "TABLE" });

			oleDbConnection.Close();

			return schemaTable;
		}
	}
}
