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
	using global::Common.Logging;

	/// Class <c>OleDbSchema.</c>
	/// <summary>
	/// Represents an OleDbSchema object.
	/// </summary>
#if NET5_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public class OleDbSchema : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
		/// Creates a new Column instance by extracting and formatting column
		/// metadata from the specified DataRow.
		/// </summary>
		/// <remarks>The returned Column reflects the schema information
		/// present in the DataRow, including type, length, nullability,
		/// default value, and position. The method expects the DataRow to
		/// follow a specific schema layout, such as that returned from
		/// database schema queries.</remarks>
		/// <param name="row">The DataRow containing column metadata. Must
		/// include fields such as COLUMN_NAME, DATA_TYPE, and other relevant
		/// schema information.</param>
		/// <returns>A Column object populated with properties derived from
		/// the values in the provided DataRow.</returns>
		public static Column FormatColumnFromDataRow(DataRow row)
		{
			Column column = null;

			if (row != null)
			{
				column = new ();
				column.Name = row["COLUMN_NAME"].ToString();

				switch ((int)row["DATA_TYPE"])
				{
					case 3: // Number
						column.ColumnType = ColumnType.Number;
						break;
					case 130: // String
						string flags = row["COLUMN_FLAGS"].ToString();

						if (int.Parse(flags, CultureInfo.InvariantCulture) > 127)
						{
							column.ColumnType = ColumnType.Memo;
						}
						else
						{
							column.ColumnType = ColumnType.String;
						}

						break;
					case 7: // Date
						column.ColumnType = ColumnType.DateTime;
						break;
					case 6: // Currency
						column.ColumnType = ColumnType.Currency;
						break;
					case 11: // Yes/No
						column.ColumnType = ColumnType.YesNo;
						break;
					case 128: // OLE
						column.ColumnType = ColumnType.Ole;
						break;
				}

				if (!row.IsNull("CHARACTER_MAXIMUM_LENGTH"))
				{
					string maxLength =
						row["CHARACTER_MAXIMUM_LENGTH"].ToString();

					column.Length =
						int.Parse(maxLength, CultureInfo.InvariantCulture);
				}

				if (row["IS_NULLABLE"].ToString() == "True")
				{
					column.Nullable = true;
				}

				if (row["COLUMN_HASDEFAULT"].ToString() == "True")
				{
					column.DefaultValue = row["COLUMN_DEFAULT"].ToString();
				}

				string position = row["ORDINAL_POSITION"].ToString();
				column.Position =
					int.Parse(position, CultureInfo.InvariantCulture);
			}

			return column;
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
		public Table GetTable(string tableName)
		{
			Table table = new(tableName);

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
		/// Sets the primary key for the specified table based on the
		/// information provided in the given data row.
		/// </summary>
		/// <remarks>If the primary key column is of integer type, its type is
		/// set to AutoNumber. This method currently supports only single
		/// -column primary keys; composite primary keys are
		/// not handled.</remarks>
		/// <param name="row">The data row containing table metadata,
		/// including the table name and primary key information. Must not be
		/// null and must contain valid 'TABLE_NAME' and 'COLUMN_NAME'
		/// fields.</param>
		/// <returns>A Table object with its PrimaryKey property set according
		/// to the primary key defined in the data row.</returns>
		public Table SetPrimaryKey(DataRow row)
		{
			Table table;

			if (row == null)
			{
				throw new ArgumentNullException(nameof(row));
			}
			else
			{
				object nameRaw = row["TABLE_NAME"];
				string tableName = nameRaw.ToString();

				table = GetTable(tableName);

				DataTable primaryKeys = GetPrimaryKeys(tableName);

				// TODO: This assumes only a single primary key.  Need to
				// compensate for composite primary keys.
				DataRow primaryKeyRow = primaryKeys.Rows[0];
				nameRaw = primaryKeyRow["COLUMN_NAME"];
				table.PrimaryKey = nameRaw.ToString();

				// If PK is an integer change type to AutoNumber
				Column primaryKey = SetPrimaryKeyType(table);

				if (primaryKey != null)
				{
					table.Columns[table.PrimaryKey] = primaryKey;
				}
			}

			return table;
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
				oleDbConnection?.Close();
				oleDbConnection = null;
			}
		}

		// If primary key is an integer, change type to AutoNumber.
		private static Column SetPrimaryKeyType(Table table)
		{
			Column primaryKey = null;

			string primaryKeyName = table.PrimaryKey;
			if (!string.IsNullOrWhiteSpace(primaryKeyName))
			{
				primaryKey = table.Columns[primaryKeyName];

				if (primaryKey.ColumnType == ColumnType.Number)
				{
					primaryKey.ColumnType = ColumnType.AutoNumber;
				}
			}

			return primaryKey;
		}
	}
}
