/////////////////////////////////////////////////////////////////////////////
// <copyright file="OleDbSchema.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Data.OleDb;
	using System.Globalization;
	using System.IO;
	using System.Runtime.Versioning;
	using global::Common.Logging;

	/// Class <c>OleDbSchema.</c>
	/// <summary>
	/// Represents an OleDbSchema object.
	/// </summary>
#if NET5_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public class OleDbSchema : DataStoreStructure
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Represents an OleDb open connection to a data source.
		/// </summary>
		private OleDbConnection oleDbConnection;

		private HashSet<string> primaryKeyNames;

		/// <summary>
		/// Initializes a new instance of the <see cref="OleDbSchema"/> class.
		/// </summary>
		/// <param name="databaseFile">The database file to use.</param>
		public OleDbSchema(string databaseFile)
			: base(DatabaseType.OleDb, databaseFile)
		{
			Log.Info("Initializing OleDbSchema.");

			string connectionString =
				OleDbHelper.BuildConnectionString(databaseFile);

			oleDbConnection = new OleDbConnection(connectionString);
		}

		/// <summary>
		/// Gets all table names from the connected database.
		/// </summary>
		/// <returns>DataTable.</returns>
		/// <value>
		/// All table names from the connected database.
		/// </value>
		public override DataTable TableNames
		{
			get
			{
				object[] restrictions = [null, null, null, "TABLE"];

				DataTable schemaTable = GetSchema(
					OleDbSchemaGuid.Tables, restrictions);

				return schemaTable;
			}
		}

		/// <summary>
		/// Gets the constraints from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public override DataTable GetConstraints(string tableName)
		{
			object[] restrictions = [null, null, tableName, null, null, null];

			DataTable schemaTable = GetSchema(
				OleDbSchemaGuid.Constraint_Column_Usage, restrictions);

			return schemaTable;
		}

		/// <summary>
		/// Gets the foreign keys from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public override DataTable GetForeignKeys(string tableName)
		{
			object[] restrictions = [null, null, tableName];

			DataTable schemaTable =
				GetSchema(OleDbSchemaGuid.Foreign_Keys, restrictions);

			return schemaTable;
		}

		/// <summary>
		/// Gets the primary keys from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public override DataTable GetPrimaryKeys(string tableName)
		{
			object[] restrictions = [null, null, tableName];

			DataTable schemaTable =
				GetSchema(OleDbSchemaGuid.Primary_Keys, restrictions);

			return schemaTable;
		}

		/// <summary>
		/// Retrieves a table definition, including its columns, for the
		/// specified table name.
		/// </summary>
		/// <remarks>The returned <see cref="Table"/> includes all columns as
		/// defined in the data source. If the table does not exist, the
		/// result may be an empty table definition.</remarks>
		/// <param name="tableName">The name of the table to retrieve. Cannot
		/// be null or empty.</param>
		/// <returns>A <see cref="Table"/> object representing the specified
		/// table and its columns.</returns>
		public override Table GetTable(string tableName)
		{
			primaryKeyNames = GetPrimaryKeyNames(tableName);

			Table table = new (tableName);

			Log.Info("Getting Columns for " + tableName);
			DataTable dataColumns = GetTableColumns(tableName);

			foreach (DataRow dataColumn in dataColumns.Rows)
			{
				Column column = FormatColumnFromDataRow(dataColumn);

				table.AddColumn(column);
			}

			return table;
		}

		/// <summary>
		/// Gets the column names from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public override DataTable GetTableColumns(string tableName)
		{
			object[] restrictions = [null, null, tableName];

			DataTable schemaTable =
				GetSchema(OleDbSchemaGuid.Columns, restrictions);

			return schemaTable;
		}

		/// <summary>
		/// Gets the constraints from the given table.
		/// </summary>
		/// <returns>DataTable.</returns>
		/// <param name="tableName">The name of the table.</param>
		public DataTable GetTableConstraints(string tableName)
		{
			object[] restrictions = [null, null, null, null, null, tableName];

			DataTable schemaTable =
				GetSchema(OleDbSchemaGuid.Table_Constraints, restrictions);

			return schemaTable;
		}

		/// <summary>
		/// Dispose.
		/// </summary>
		/// <param name="disposing">Indicates whether the object is
		/// currently disposing.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				oleDbConnection?.Close();
				oleDbConnection = null;
			}
		}

		/// <summary>
		/// Determines the column type based on the specified data type name and
		/// optional flags.
		/// </summary>
		/// <param name="dataType">The name of the data type to evaluate. Common
		/// values include "integer", "string", and "date". Cannot be null.
		/// </param>
		/// <param name="flags">Optional flags that influence the column type
		/// determination. The meaning of specific flag values depends on the
		/// implementation.</param>
		/// <param name="length">The length of the data type,
		/// if applicable.</param>
		/// <returns>A value from the <see cref="ColumnType"/> enumeration that
		/// represents the inferred column type. Returns <see
		/// cref="ColumnType.Unknown"/> if the data type is not recognized.
		/// </returns>
		protected override ColumnType GetColumnType(
			string dataType, int flags, int length)
		{
			ColumnType columnType = ColumnType.Unknown;
			int dataTypeValue = int.TryParse(dataType, out int temp) ? temp : 0;

			switch (dataTypeValue)
			{
				case 3: // Number
					columnType = ColumnType.Number;
					break;
				case 130: // String
					if (length > 255)
					{
						columnType = ColumnType.Memo;
					}
					else
					{
						columnType = ColumnType.String;
					}

					break;
				case 7: // Date
					columnType = ColumnType.DateTime;
					break;
				case 6: // Currency
					columnType = ColumnType.Currency;
					break;
				case 11: // Yes/No
					columnType = ColumnType.YesNo;
					break;
				case 128: // OLE
					columnType = ColumnType.Ole;
					break;
			}

			return columnType;
		}

		/// <summary>
		/// Creates a <see cref="Relationship"/> object representing the foreign
		/// key relationship described by the specified data row.
		/// </summary>
		/// <remarks>The method expects the <paramref name="foreignKey"/> to
		/// contain specific column names such as "CONSTRAINT_NAME",
		/// "TABLE_NAME", "FKEY_FROM_COLUMN", "FKEY_TO_TABLE", "FKEY_TO_COLUMN",
		/// "FKEY_ON_DELETE", and "FKEY_ON_DELETE". If these columns are missing
		/// or contain unexpected values, the resulting <see
		/// cref="Relationship"/> may not be fully populated.</remarks>
		/// <param name="foreignKey">A <see cref="DataRow"/> containing metadata
		/// about a foreign key constraint. Must include columns for constraint
		/// name, table names, column names, and update/delete rules.</param>
		/// <returns>A <see cref="Relationship"/> instance populated with
		/// information about the foreign key relationship, including
		/// table and column names, and cascade rules.</returns>
		protected override Relationship GetRelationship(DataRow foreignKey)
		{
			Relationship relationship = new ();

			// Note: OleDb seems to have reversed referencing.
			const string constraintNameKey = "FK_NAME";
			const string parentTableNameKey = "FK_TABLE_NAME";
			const string parentColumnNameKey = "FK_COLUMN_NAME";
			const string childTableNameKey = "PK_TABLE_NAME";
			const string childColumnNameKey = "PK_COLUMN_NAME";
			const string updateRuleKey = "UPDATE_RULE";
			const string deleteRuleKey = "DELETE_RULE";

			if (foreignKey != null)
			{
				relationship.Name = foreignKey[constraintNameKey].ToString();
				relationship.ParentTable =
					foreignKey[parentTableNameKey].ToString();
				relationship.ParentTableCol =
					foreignKey[parentColumnNameKey].ToString();
				relationship.ChildTable =
					foreignKey[childTableNameKey].ToString();
				relationship.ChildTableCol =
					foreignKey[childColumnNameKey].ToString();

				string onAction = foreignKey[deleteRuleKey].ToString();

				if (onAction.Equals("CASCADE", StringComparison.Ordinal))
				{
					relationship.OnDeleteAction = ConstraintAction.Cascade;
				}
				else if (onAction.Equals("SET NULL", StringComparison.Ordinal))
				{
					relationship.OnDeleteAction = ConstraintAction.SetNull;
				}
				else
				{
					relationship.OnDeleteAction = ConstraintAction.NoAction;
				}

				onAction = foreignKey[updateRuleKey].ToString();

				if (onAction.Equals("CASCADE", StringComparison.Ordinal))
				{
					relationship.OnUpdateAction = ConstraintAction.Cascade;
				}
				else if (onAction.Equals("SET NULL", StringComparison.Ordinal))
				{
					relationship.OnUpdateAction = ConstraintAction.SetNull;
				}
				else
				{
					relationship.OnUpdateAction = ConstraintAction.NoAction;
				}
			}

			return relationship;
		}

		/// <summary>
		/// Determines whether the specified column is marked as a primary key.
		/// </summary>
		/// <remarks>The method checks the value of the "PRIMARY_KEY" field in
		/// the provided <see cref="DataRow"/>. The comparison is case-sensitive
		/// and expects the value to be exactly "True" to indicate a primary
		/// key.</remarks>
		/// <param name="column">The <see cref="DataRow"/> representing the
		/// column to evaluate. Cannot be null.</param>
		/// <returns>true if the column is designated as a primary key;
		/// otherwise, false.</returns>
		protected override bool IsPrimaryKey(DataRow column)
		{
			bool isPrimaryKey = false;

			ArgumentNullException.ThrowIfNull(column);

			if (column.Table.Columns.Contains("COLUMN_NAME"))
			{
				string columnName = column["COLUMN_NAME"].ToString();

				bool exists = primaryKeyNames.Contains(columnName);

				if (exists == true)
				{
					isPrimaryKey = true;
				}
			}

			return isPrimaryKey;
		}

		private HashSet<string> GetPrimaryKeyNames(string tableName)
		{
			DataTable primaryKeys = GetPrimaryKeys(tableName);

			primaryKeyNames = [];

			foreach (DataRow row in primaryKeys.Rows)
			{
				string primaryKeyName = row["COLUMN_NAME"].ToString();

				primaryKeyNames.Add(primaryKeyName);
			}

			return primaryKeyNames;
		}

		private DataTable GetSchema(Guid guid, object[] restrictions)
		{
			if (oleDbConnection.State != ConnectionState.Open)
			{
				oleDbConnection.Open();
			}

			DataTable schemaTable =
				oleDbConnection.GetOleDbSchemaTable(guid, restrictions);

			oleDbConnection.Close();

			return schemaTable;
		}
	}
}
