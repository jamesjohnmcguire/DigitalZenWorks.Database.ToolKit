/////////////////////////////////////////////////////////////////////////////
// <copyright file="OleDbSchema.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Data;
	using System.Data.OleDb;
	using System.Globalization;
	using System.Runtime.Versioning;

	/// Class <c>OleDbSchema.</c>
	/// <summary>
	/// Represents an OleDbSchema object.
	/// </summary>
#if NET5_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public class OleDbSchema : IDisposable
	{
		/// <summary>
		/// Represents an OleDb open connection to a data source.
		/// </summary>
		private OleDbConnection oleDbConnection;

		/// <summary>
		/// Initializes a new instance of the <see cref="OleDbSchema"/> class.
		/// </summary>
		/// <param name="databaseFile">The database file to use.</param>
		public OleDbSchema(string databaseFile)
		{
			string baseFormat = "Provider=Microsoft.ACE.OLEDB.12.0" +
				@";Password="""";User ID=Admin;" + "Data Source={0}" +
				";Mode=Share Deny None;" +
				@"Extended Properties="""";" +
				@"Jet OLEDB:System database="""";" +
				@"Jet OLEDB:Registry Path="""";" +
				@"Jet OLEDB:Database Password="""";Jet OLEDB:Engine Type=5;" +
				@"Jet OLEDB:New Database Password="""";" +
				"Jet OLEDB:Database Locking Mode=1;" +
				"Jet OLEDB:Global Partial Bulk Ops=2;" +
				"Jet OLEDB:Global Bulk Transactions=1;" +
				"Jet OLEDB:Create System Database=False;" +
				"Jet OLEDB:Encrypt Database=False;" +
				"Jet OLEDB:Don't Copy Locale on Compact=False;" +
				"Jet OLEDB:Compact Without Replica Repair=False;" +
				"Jet OLEDB:SFP=False";

			string connectionString = string.Format(
				CultureInfo.InvariantCulture,
				baseFormat,
				databaseFile);

			oleDbConnection = new OleDbConnection(connectionString);
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
				oleDbConnection.Open();

				object[] testTable = [null, null, null, "TABLE"];

				DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
					OleDbSchemaGuid.Tables, testTable);

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

		/// <summary>
		/// Gets the constraints from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetConstraints(string tableName)
		{
			oleDbConnection.Open();

			object[] testTable = [null, null, tableName, null, null, null];

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				OleDbSchemaGuid.Constraint_Column_Usage, testTable);

			oleDbConnection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Gets the foreign keys from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetForeignKeys(string tableName)
		{
			oleDbConnection.Open();

			object[] testTable = [null, null, tableName];

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				OleDbSchemaGuid.Foreign_Keys, testTable);

			oleDbConnection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Gets the primary keys from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetPrimaryKeys(string tableName)
		{
			oleDbConnection.Open();

			object[] testTable = [null, null, tableName];

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				OleDbSchemaGuid.Primary_Keys, testTable);

			oleDbConnection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Gets the column names from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetTableColumns(string tableName)
		{
			oleDbConnection.Open();

			object[] testTable = [null, null, tableName];

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				OleDbSchemaGuid.Columns, testTable);

			oleDbConnection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Gets the constraints from the given table.
		/// </summary>
		/// <returns>DataTable.</returns>
		/// <param name="tableName">The name of the table.</param>
		public DataTable GetTableConstraints(string tableName)
		{
			oleDbConnection.Open();

			object[] testTable = [null, null, null, null, null, tableName];

			DataTable schemaTable = oleDbConnection.GetOleDbSchemaTable(
				OleDbSchemaGuid.Table_Constraints, testTable);

			oleDbConnection.Close();

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
				if (oleDbConnection != null)
				{
					oleDbConnection.Close();
					oleDbConnection = null;
				}
			}
		}
	}
}
