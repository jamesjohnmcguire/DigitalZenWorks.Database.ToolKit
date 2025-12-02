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

		/// <summary>
		/// Initializes a new instance of the <see cref="OleDbSchema"/> class.
		/// </summary>
		/// <param name="databaseFile">The database file to use.</param>
		public OleDbSchema(string databaseFile)
			: base(DatabaseType.OleDb, databaseFile)
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
		/// Retrieves the database schema as a collection of tables, including
		/// primary and foreign key information.
		/// </summary>
		/// <remarks>Each <see cref="Table"/> in the returned collection
		/// includes its primary key and foreign key relationships. Use this
		/// method to obtain a complete representation of the database
		/// structure for schema inspection or metadata operations.</remarks>
		/// <returns>A collection of <see cref="Table"/> objects representing
		/// all tables in the database schema. The collection will be empty if
		/// no tables are found.</returns>
		public override Collection<Table> GetSchema()
		{
			Dictionary<string, Table> tableDictionary = [];
			Collection<Relationship> relationships = [];

			foreach (DataRow row in TableNames.Rows)
			{
				string tableName = GetTableName(row);
				Table table = GetTable(row);
				tableDictionary.Add(tableName, table);

				relationships = GetRelationships(tableName, relationships);
			}

			tableDictionary =
				SetTablesRelationships(tableDictionary, relationships);

			Collection<Table> tables = GetTables(tableDictionary);

			return tables;
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
				string tableName = GetTableName(row);

				table = GetTable(tableName);

				DataTable primaryKeys = GetPrimaryKeys(tableName);

				// TODO: This assumes only a single primary key.  Need to
				// compensate for composite primary keys.
				DataRow primaryKeyRow = primaryKeys.Rows[0];
				object nameRaw = primaryKeyRow["COLUMN_NAME"];
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
		/// Generates the SQL definition string for the specified column,
		/// including its name, type, constraints, and default value.
		/// </summary>
		/// <remarks>The generated SQL includes the column name, type, and
		/// applicable constraints such as UNIQUE, NOT NULL, IDENTITY, and
		/// DEFAULT. The output is intended for use in table creation scripts
		/// and ends with a comma and newline.</remarks>
		/// <param name="column">The column for which to generate the SQL
		/// definition. Cannot be null.</param>
		/// <returns>A string containing the SQL definition for the column,
		/// formatted for inclusion in a CREATE TABLE statement.</returns>
		protected override string GetColumnSql(Column column)
		{
			ArgumentNullException.ThrowIfNull(column);

			string sql = "\t[" + column.Name + "]";

			string columnType = GetColumnTypeText(column);
			sql += columnType;

			if (column.Unique)
			{
				sql += " UNIQUE";
			}

			if (!column.Nullable)
			{
				sql += " NOT NULL";
			}

			if (column.ColumnType == ColumnType.AutoNumber)
			{
				sql += " IDENTITY";
			}

			if (!string.IsNullOrWhiteSpace(column.DefaultValue))
			{
				sql += " DEFAULT " + column.DefaultValue;
			}

			sql += "," + Environment.NewLine;

			return sql;
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
		/// Generates a SQL CREATE TABLE statement for the specified table
		/// definition.
		/// </summary>
		/// <remarks>The generated SQL statement includes all columns in ordinal
		/// position order, as well as primary key and foreign key constraints
		/// if specified in the table definition. The output uses double quotes
		/// for table and column names to ensure compatibility with the ANSI
		/// standard syntax.</remarks>
		/// <param name="table">The table structure containing column
		/// definitions, primary key, and foreign keys to be used in the
		/// generated SQL statement. Cannot be null.</param>
		/// <returns>A string containing the SQL CREATE TABLE statement that
		/// defines the table, its columns, primary key, and foreign key
		/// constraints.</returns>
		protected override string GetCreateTableStatement(Table table)
		{
			ArgumentNullException.ThrowIfNull(table);

			string sql = string.Format(
				CultureInfo.InvariantCulture,
				"CREATE TABLE [{0}] ({1}",
				table.Name,
				Environment.NewLine);

			SortedList<int, Column> columns = GetOrdinalSortedColumns(table);

			foreach (KeyValuePair<int, Column> entry in columns)
			{
				Column column = entry.Value;

				sql += GetColumnSql(column);
			}

			if (!string.IsNullOrWhiteSpace(table.PrimaryKey))
			{
				sql += string.Format(
					CultureInfo.InvariantCulture,
					"\tCONSTRAINT PrimaryKey PRIMARY KEY ([{0}]),{1}",
					table.PrimaryKey,
					Environment.NewLine);
			}

			for (int index = 0; index < table.ForeignKeys.Count; index++)
			{
				ForeignKey foreignKey = table.ForeignKeys[index];

				bool isLast = false;

				if (index == table.ForeignKeys.Count - 1)
				{
					isLast = true;
				}

				sql += GetForeignKeySql(foreignKey, isLast);
			}

			sql += string.Format(
				CultureInfo.InvariantCulture,
				"{0});{0}",
				Environment.NewLine);

			return sql;
		}

		/// <summary>
		/// Generates the SQL statement for a foreign key constraint based on
		/// the specified foreign key definition.
		/// </summary>
		/// <remarks>The generated SQL includes ON DELETE CASCADE and ON UPDATE
		/// CASCADE clauses if the corresponding options are set in the foreign
		/// key definition. The output is formatted for inclusion in a CREATE
		/// TABLE statement.</remarks>
		/// <param name="foreignKey">The foreign key definition containing the
		/// constraint name, parent column, child table, child column, and
		/// cascade options. Cannot be null.</param>
		/// <param name="isLast">Indicates whether this constraint is the last
		/// in the list. If <see langword="false"/>, a comma is appended to the
		/// SQL statement.</param>
		/// <returns>A string containing the SQL statement for the foreign key
		/// constraint, including cascade options if specified.</returns>
		protected override string GetForeignKeySql(
			ForeignKey foreignKey, bool isLast)
		{
			ArgumentNullException.ThrowIfNull(foreignKey);

			string constraint = "CONSTRAINT";
			string key = "FOREIGN KEY";
			string references = "REFERENCES";

			string statement = "\t{0} [{1}] {2} ([{3}]) {4} [{5}] ([{6}])";

			string sql = string.Format(
				CultureInfo.InvariantCulture,
				statement,
				constraint,
				foreignKey.Name,
				key,
				foreignKey.ParentColumn,
				references,
				foreignKey.ChildTable,
				foreignKey.ChildColumn);

			if (foreignKey.CascadeOnDelete)
			{
				sql += " ON DELETE CASCADE";
			}

			if (foreignKey.CascadeOnUpdate)
			{
				sql += " ON UPDATE CASCADE";
			}

			if (isLast == false)
			{
				sql += ",";
			}

			sql += Environment.NewLine;

			return sql;
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
			string constraintNameKey = "FK_NAME";
			string parentTableNameKey = "FK_TABLE_NAME";
			string parentColumnNameKey = "FK_COLUMN_NAME";
			string childTableNameKey = "PK_TABLE_NAME";
			string childColumnNameKey = "PK_COLUMN_NAME";
			string updateRuleKey = "UPDATE_RULE";
			string deleteRuleKey = "DELETE_RULE";

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

				if (foreignKey[updateRuleKey].ToString() != "NO ACTION")
				{
					relationship.OnUpdateCascade = true;
				}

				if (foreignKey[deleteRuleKey].ToString() != "NO ACTION")
				{
					relationship.OnDeleteCascade = true;
				}
			}

			return relationship;
		}

		/// <summary>
		/// Retrieves the parent table specified by the relationship and adds
		/// the corresponding foreign key to its collection.
		/// </summary>
		/// <remarks>If the specified relationship or table dictionary is null,
		/// the method returns null and no changes are made. The foreign key is
		/// added to the parent table's <c>ForeignKeys</c> collection.
		/// </remarks>
		/// <param name="tableDictionary">A dictionary containing table names
		/// as keys and their corresponding <see cref="Table"/> objects as
		/// values. Must not be null.</param>
		/// <param name="relationship">The relationship that defines the
		/// parent table and foreign key to associate. Must not be null.
		/// </param>
		/// <returns>The <see cref="Table"/> object representing the parent
		/// table with the foreign key added, or <see langword="null"/>
		/// if <paramref name="tableDictionary"/> or
		/// <paramref name="relationship"/> is null.</returns>
		protected override Table GetTableWithRelationships(
			Dictionary<string, Table> tableDictionary,
			Relationship relationship)
		{
			Table table = null;

			if (tableDictionary != null && relationship != null)
			{
				string name = relationship.ParentTable;

				ForeignKey foreignKey =
					GetForeignKeyRelationship(relationship);

				table = tableDictionary[name];

				table.ForeignKeys.Add(foreignKey);
			}

			return table;
		}

		/// <summary>
		/// Order table.
		/// </summary>
		/// <param name="tables">The list of tables to order.</param>
		/// <returns>The ordered list of of tables.</returns>
		/// <remarks>This orders the list taking dependencies into
		/// account.</remarks>
		protected override Collection<string> OrderTable(
			Collection<Table> tables)
		{
			Collection<string> orderedTables = [];

			if (tables != null)
			{
				Dictionary<string, Collection<string>> tableDependencies = [];

				foreach (Table table in tables)
				{
					Collection<string> dependencies = [];
					string name = table.Name;

					foreach (ForeignKey foreignKeys in table.ForeignKeys)
					{
						dependencies.Add(foreignKeys.ParentTable);
					}

					tableDependencies.Add(name, dependencies);
				}

				orderedTables = GetOrderedDependencies(tableDependencies);
			}

			return orderedTables;
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

		private Table GetTable(DataRow row)
		{
			Table table = SetPrimaryKey(row);

			return table;
		}
	}
}
