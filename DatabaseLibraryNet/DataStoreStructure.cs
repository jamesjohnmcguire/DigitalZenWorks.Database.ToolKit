/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataStoreStructure.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Data.Common;
	using System.Globalization;
	using global::Common.Logging;

	/// Class <c>DataStoreStructure.</c>
	/// <summary>
	/// Represents a database schema.
	/// </summary>
	public class DataStoreStructure : IDisposable
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Represents the database type.
		/// </summary>
		private readonly DatabaseType databaseType = DatabaseType.Unknown;

		/// <summary>
		/// Represents a connection to a data source.
		/// </summary>
		private readonly DbConnection connection;

		/// <summary>
		/// The core database object.
		/// </summary>
		private DataStorage database;

		/// <summary>
		/// Initializes a new instance of the
		/// <see cref="DataStoreStructure"/> class.
		/// </summary>
		/// <param name="databaseType">The database type.</param>
		/// <param name="databaseFile">The database file to use.</param>
		public DataStoreStructure(
			DatabaseType databaseType, string databaseFile)
		{
			this.databaseType = databaseType;

			string connectionString =
				DataStorage.GetConnectionString(databaseType, databaseFile);

			database = new (databaseType, connectionString);
			database.Open();

			connection = database.Connection;
		}

		/// <summary>
		/// Gets all table names from the connected database.
		/// </summary>
		/// <returns>DataTable.</returns>
		/// <value>
		/// All table names from the connected database.
		/// </value>
		public virtual DataTable TableNames
		{
			get
			{
				DataTable schemaTable = GetSchema("Tables", null);

				return schemaTable;
			}
		}

		/// <summary>
		/// Gets the foreign key relationships.
		/// </summary>
		/// <param name="relationships">A set of relationships.</param>
		/// <returns>The foreign key relationships.</returns>
		public static ForeignKey[] GetForeignKeyRelationships(
			Relationship[] relationships)
		{
			ForeignKey[] keys = null;

			if (relationships != null)
			{
				int count = 0;
				keys = new ForeignKey[relationships.Length];

				// Add foreign keys to table, using relationships
				foreach (Relationship relationship in relationships)
				{
					ForeignKey foreignKey =
						GetForeignKeyRelationship(relationship);

					keys[count] = foreignKey;
					count++;
				}
			}

			return keys;
		}

		/// <summary>
		/// Get ordered dependencies.
		/// </summary>
		/// <param name="tableDependencies">A collection of table
		/// depdenencies.</param>
		/// <returns>A list of ordered dependencies.</returns>
		public static Collection<string> GetOrderedDependencies(
			Dictionary<string, Collection<string>> tableDependencies)
		{
			Collection<string> orderedDependencies = [];

			// Tracks previously processed nodes.
			HashSet<string> visited = [];

			// Tracks nodes in the current recursion stack
			// (for cycle detection).
			HashSet<string> visiting = [];

			if (tableDependencies != null)
			{
				foreach (string key in tableDependencies.Keys)
				{
					if (!visited.Contains(key))
					{
						GetDependenciesRecursive(
							key,
							tableDependencies,
							orderedDependencies,
							visited,
							visiting);
					}
				}
			}

			return orderedDependencies;
		}

		/// <summary>
		/// Retrieves the table name from the specified <see cref="DataRow"/>
		/// instance.
		/// </summary>
		/// <param name="row">The <see cref="DataRow"/> containing a value for
		/// the "TABLE_NAME" column. Must not be null and must contain the
		/// "TABLE_NAME" column.</param>
		/// <returns>A string representing the table name extracted from the
		/// "TABLE_NAME" column of the provided <see cref="DataRow"/>.</returns>
		public static string GetTableName(DataRow row)
		{
			ArgumentNullException.ThrowIfNull(row);

			object nameRaw = row["TABLE_NAME"];
			string tableName = nameRaw.ToString();

			return tableName;
		}

		/// <summary>
		/// Replaces the foreign key definitions of the specified table with
		/// those derived from the provided relationships.
		/// </summary>
		/// <remarks>All existing foreign keys in the table are cleared before
		/// new ones are added. The method does not modify the input
		/// relationships list.</remarks>
		/// <param name="table">The table whose foreign keys will be set or
		/// updated.</param>
		/// <param name="relationships">A list of relationships from which
		/// foreign key definitions will be generated and applied to the table.
		/// Cannot be null.</param>
		/// <returns>The table instance with its foreign keys updated to
		/// reflect the specified relationships.</returns>
		public static Table SetForeignKeys(
			Table table, Collection<Relationship> relationships)
		{
			if (table == null)
			{
				throw new ArgumentNullException(
					nameof(table),
					"Table cannot be null");
			}

			if (relationships != null)
			{
				table.ForeignKeys.Clear();

				foreach (Relationship relationship in relationships)
				{
					ForeignKey foreignKey =
						GetForeignKeyRelationship(relationship);

					table.ForeignKeys.Add(foreignKey);
				}
			}

			return table;
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
		public virtual Column FormatColumnFromDataRow(DataRow row)
		{
			Column column = null;

			if (row != null)
			{
				column = new ();

				column.Name = row["COLUMN_NAME"].ToString();
				string dataType = row["DATA_TYPE"].ToString();

				string flagsText = row["COLUMN_FLAGS"].ToString();
				int flags = int.TryParse(flagsText, out int temp) ? temp : 0;
				int length = GetColumnLength(row);
				column.Length = length;

				column.ColumnType = GetColumnType(dataType, length, flags);

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
					int.TryParse(position, out temp) ? temp : 0;
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
		public virtual DataTable GetConstraints(string tableName)
		{
			DataTable constraints = GetBaseConstraints();

			constraints = AddForeignKeyConstraints(
				tableName, constraints);

			constraints = AddIndexConstraints(
				tableName, constraints);

			return constraints;
		}

		/// <summary>
		/// Gets the foreign keys from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public virtual DataTable GetForeignKeys(string tableName)
		{
			string[] testTable = [null, null, tableName];

			DataTable schemaTable = GetSchema("ForeignKeys", testTable);

			return schemaTable;
		}

		/// <summary>
		/// Gets the constraints from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetIndexes(string tableName)
		{
			string[] tableInformation = [null, null, tableName];

			DataTable schemaTable = GetSchema("Indexes", tableInformation);

			return schemaTable;
		}

		/// <summary>
		/// Gets the primary keys from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public virtual DataTable GetPrimaryKeys(string tableName)
		{
			string[] tableInformation = [null, null, tableName];

			DataTable schemaTable = GetSchema("PrimaryKeys", tableInformation);

			return schemaTable;
		}

		/// <summary>
		/// Gets a list of relationships.
		/// </summary>
		/// <param name="tableName">The table name.</param>
		/// <param name="relationships">A list of relationships.</param>
		/// <returns>An updated list of relationships.</returns>
		public Collection<Relationship> GetRelationships(
			string tableName, Collection<Relationship> relationships)
		{
			relationships ??= [];

			DataTable foreignKeyTable = GetForeignKeys(tableName);

			foreach (DataRow foreignKey in foreignKeyTable.Rows)
			{
				Relationship relationship = GetRelationship(foreignKey);

				relationships.Add(relationship);
			}

			return relationships;
		}

		/// <summary>
		/// Retrieves the schema information for all tables in the specified
		/// database file, including their relationships and foreign keys.
		/// </summary>
		/// <remarks>The returned collection includes all tables found in the
		/// database, with foreign key relationships established based on the
		/// detected schema. If the database file does not exist or is
		/// inaccessible, an exception may be thrown.</remarks>
		/// <returns>A collection of <see cref="Table"/> objects representing
		/// the tables in the database, each populated with its foreign key
		/// relationships.</returns>
		public virtual Collection<Table> GetSchema()
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
		public virtual DataTable GetTableColumns(string tableName)
		{
			string[] tableInformation = [null, null, tableName];

			DataTable schemaTable = GetSchema("Columns", tableInformation);

			return schemaTable;
		}

		/// <summary>
		/// Returns the SQL type declaration string corresponding to the
		/// specified column's type and length.
		/// </summary>
		/// <remarks>The returned string includes the appropriate SQL type and,
		/// for string columns, the length constraint. This method does not
		/// validate the column's properties; callers should ensure that the
		/// column is properly configured before calling.</remarks>
		/// <param name="column">The column for which to generate the SQL type
		/// declaration. The column's type and length determine the returned
		/// string.</param>
		/// <returns>A string representing the SQL type declaration for the
		/// column. Returns an empty string if the column type is not
		/// recognized.</returns>
		protected static string GetColumnTypeText(Column column)
		{
			ArgumentNullException.ThrowIfNull(column);

			string columnType = column.ColumnType switch
			{
				ColumnType.Number => " INTEGER",
				ColumnType.AutoNumber => " INTEGER",
				ColumnType.String => string.Format(
					CultureInfo.InvariantCulture,
					" VARCHAR({0})",
					column.Length),
				ColumnType.Memo => " MEMO",
				ColumnType.DateTime => " DATETIME",
				ColumnType.Currency => " CURRENCY",
				ColumnType.Ole => " OLEOBJECT",
				ColumnType.YesNo => " OLEOBJECT",
				_ => string.Empty,
			};

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
		protected virtual string GetCreateTableStatement(Table table)
		{
			ArgumentNullException.ThrowIfNull(table);

			string sql = string.Format(
				CultureInfo.InvariantCulture,
				"CREATE TABLE `{0}` ({1}",
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
					"\tCONSTRAINT PrimaryKey PRIMARY KEY (\"{0}\"),{1}",
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
		/// Creates a <see cref="ForeignKey"/> instance that represents the
		/// foreign key relationship defined by the specified
		/// <see cref="Relationship"/> object.
		/// </summary>
		/// <param name="relationship">The <see cref="Relationship"/> object
		/// containing the details of the foreign key relationship to be
		/// represented. Cannot be null.</param>
		/// <returns>A <see cref="ForeignKey"/> instance initialized with the
		/// properties of the specified <paramref name="relationship"/>.
		/// </returns>
		protected static ForeignKey GetForeignKeyRelationship(
			Relationship relationship)
		{
			ForeignKey foreignKey;

			if (relationship == null)
			{
				throw new ArgumentNullException(
					nameof(relationship),
					"Relationship cannot be null");
			}
			else
			{
				foreignKey = new(
					relationship.Name,
					relationship.ParentTable,
					relationship.ParentTableCol,
					relationship.ChildTable,
					relationship.ChildTableCol,
					relationship.OnDeleteCascade,
					relationship.OnUpdateCascade);
			}

			return foreignKey;
		}

		/// <summary>
		/// Returns a sorted list of columns from the specified table, ordered
		/// by their ordinal position.
		/// </summary>
		/// <remarks>The returned list is sorted in ascending order by column
		/// position. If the table contains no columns, the returned list will
		/// be empty.</remarks>
		/// <param name="table">The table containing the columns to be sorted.
		/// Cannot be null.</param>
		/// <returns>A SortedList where each key is the ordinal position of a
		/// column and each value is the corresponding Column object from the
		/// table.</returns>
		protected static SortedList<int, Column> GetOrdinalSortedColumns(
			Table table)
		{
			ArgumentNullException.ThrowIfNull(table);

			// Sort Columns into ordinal positions
			SortedList<int, Column> columns = [];

			foreach (KeyValuePair<string, Column> entry in table.Columns)
			{
				Column column = entry.Value;
				columns.Add(column.Position, column);
			}

			return columns;
		}

		/// <summary>
		/// Creates a collection containing all tables from the specified
		/// dictionary.
		/// </summary>
		/// <param name="tableDictionary">A dictionary that maps table names
		/// to <see cref="Table"/> objects. Cannot be <see langword="null"/>.
		/// </param>
		/// <returns>A <see cref="Collection{Table}"/> containing all
		/// <see cref="Table"/> objects from the dictionary. The collection
		/// will be empty if the dictionary contains no tables.</returns>
		/// <exception cref="ArgumentNullException">Thrown if
		/// <paramref name="tableDictionary"/> is <see langword="null"/>.
		/// </exception>
		protected static Collection<Table> GetTables(
			Dictionary<string, Table> tableDictionary)
		{
			if (tableDictionary == null)
			{
				throw new ArgumentNullException(
					nameof(tableDictionary),
					"Table dictionary cannot be null");
			}

			Dictionary<string, Table>.ValueCollection values =
				tableDictionary.Values;
			List<Table> newList = [.. values];
			Collection<Table> tables = new(newList);

			return tables;
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
				database?.Close();
				database?.Dispose();
				database = null;
			}
		}

		/// <summary>
		/// Retrieves the maximum length, in characters, defined for a column
		/// represented by the specified data row.
		/// </summary>
		/// <remarks>If the 'CHARACTER_MAXIMUM_LENGTH' field is null or cannot
		/// be parsed as an integer, the method returns 0.</remarks>
		/// <param name="row">The <see cref="System.Data.DataRow"/> containing
		/// column metadata. Must include the 'CHARACTER_MAXIMUM_LENGTH' field.
		/// </param>
		/// <returns>The maximum character length of the column if specified;
		/// otherwise, 0.</returns>
		protected virtual int GetColumnLength(DataRow row)
		{
			int length = 0;

			ArgumentNullException.ThrowIfNull(row);

			bool test = row.IsNull("CHARACTER_MAXIMUM_LENGTH");

			if (test == false)
			{
				string maxLength = row["CHARACTER_MAXIMUM_LENGTH"].ToString();

				length = int.TryParse(maxLength, out int temp) ? temp : 0;
			}

			return length;
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
		protected virtual string GetColumnSql(Column column)
		{
			ArgumentNullException.ThrowIfNull(column);

			string sql = "\t\"" + column.Name + "\"";

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
		protected virtual ColumnType GetColumnType(
			string dataType, int flags, int length)
		{
			ColumnType columnType = dataType switch
			{
				"integer" => ColumnType.Number,
				"string" => ColumnType.String,

				// Guessing date vs. datetime based on name
				"date" => ColumnType.DateTime,
				_ => ColumnType.Unknown,
			};

			return columnType;
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
		protected virtual string GetForeignKeySql(
			ForeignKey foreignKey, bool isLast)
		{
			ArgumentNullException.ThrowIfNull(foreignKey);

			string constraint = "CONSTRAINT";
			string key = "FOREIGN KEY";
			string references = "REFERENCES";

			string statement =
				"\t{0} \"{1}\" {2} (\"{3}\") {4} \"{5}\" (\"{6}\")";

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
		protected virtual Relationship GetRelationship(DataRow foreignKey)
		{
			ArgumentNullException.ThrowIfNull(foreignKey);

			Relationship relationship = new ();

			// Using standard (or perhaps Sqlite) keys
			string constraintNameKey = "CONSTRAINT_NAME";
			string tableNameKey = "TABLE_NAME";
			string columnNameKey = "FKEY_FROM_COLUMN";
			string foreignTableNameKey = "FKEY_TO_TABLE";
			string foreignColumnNameKey = "FKEY_TO_COLUMN";
			string updateRuleKey = "FKEY_ON_DELETE";
			string deleteRuleKey = "FKEY_ON_DELETE";

			relationship.Name = foreignKey[constraintNameKey].ToString();
			relationship.ParentTable =
				foreignKey[tableNameKey].ToString();
			relationship.ParentTableCol =
				foreignKey[columnNameKey].ToString();
			relationship.ChildTable =
				foreignKey[foreignTableNameKey].ToString();
			relationship.ChildTableCol =
				foreignKey[foreignColumnNameKey].ToString();

			if (foreignKey[updateRuleKey].ToString() != "NO ACTION")
			{
				relationship.OnUpdateCascade = true;
			}

			if (foreignKey[deleteRuleKey].ToString() != "NO ACTION")
			{
				relationship.OnDeleteCascade = true;
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
		protected virtual Table GetTableWithRelationships(
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
		protected virtual Collection<string> OrderTable(
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

		/// <summary>
		/// Establishes foreign key relationships between tables in
		/// the specified dictionary based on the provided collection of
		/// relationships.
		/// </summary>
		/// <remarks>This method updates the tables in the dictionary to
		/// reflect the foreign key relationships defined in the collection.
		/// The input dictionary is modified in place; no new dictionary is
		/// created.</remarks>
		/// <param name="tableDictionary">A dictionary containing table
		/// objects, keyed by their names. Cannot be null.</param>
		/// <param name="relationships">A collection of relationships that
		/// define foreign key associations between tables. If null, no
		/// relationships are added.</param>
		/// <returns>The original dictionary of tables with foreign key
		/// relationships applied as specified by the relationships
		/// collection.</returns>
		/// <exception cref="ArgumentNullException">Thrown if
		/// <paramref name="tableDictionary"/> is null.</exception>
		protected Dictionary<string, Table> SetTablesRelationships(
			Dictionary<string, Table> tableDictionary,
			Collection<Relationship> relationships)
		{
			if (tableDictionary == null)
			{
				throw new ArgumentNullException(
					nameof(tableDictionary),
					"Table dictionary cannot be null");
			}

			if (relationships != null)
			{
				// Add foreign keys to tables in dictionary,
				// using relationships.
				foreach (Relationship relationship in relationships)
				{
					// Will modify tableDictionary in place.
					GetTableWithRelationships(
						tableDictionary, relationship);
				}
			}

			return tableDictionary;
		}

		private static DataTable GetBaseConstraints()
		{
			DataTable table = new();

			Type stringType = typeof(string);

			table.Columns.Add("ConstraintType", stringType);
			table.Columns.Add("ConstraintName", stringType);
			table.Columns.Add("TableName", stringType);
			table.Columns.Add("ColumnName", stringType);
			table.Columns.Add("ReferencedTable", stringType);
			table.Columns.Add("ReferencedColumn", stringType);

			return table;
		}

		private static string GetConstraintQueryMySql(string tableName)
		{
			string fields = "CONSTRAINT_TYPE, CONSTRAINT_NAME, TABLE_NAME, " +
				"COLUMN_NAME, REFERENCED_TABLE_NAME, REFERENCED_COLUMN_NAME";

			string from = "INFORMATION_SCHEMA.KEY_COLUMN_USAGE";

			string union = "UNION SELECT 'PRIMARY KEY' as CONSTRAINT_TYPE, " +
				"CONSTRAINT_NAME, TABLE_NAME, COLUMN_NAME, " +
				"NULL as REFERENCED_TABLE_NAME, " +
				"NULL as REFERENCED_COLUMN_NAME";

			string where = $"WHERE TABLE_NAME = '{tableName}' " +
				"AND CONSTRAINT_NAME != 'PRIMARY'";

			string query = $"SELECT {fields} FROM {from} " +
				$"{union} FROM {from} {where}";

			return query;
		}

		private static string GetConstraintQueryOracle(string tableName)
		{
			string fields = @"SELECT constraints.CONSTRAINT_TYPE,
				constraints.CONSTRAINT_NAME,
				constraints.TABLE_NAME,
				constraintColumns.COLUMN_NAME,
				constraints.R_CONSTRAINT_NAME,
				referentialConstraints.TABLE_NAME as REFERENCED_TABLE_NAME,
				referentialConstraintColumns.COLUMN_NAME as
				REFERENCED_COLUMN_NAME";

			string from = "ALL_CONSTRAINTS constraints";

			string joins =
				"LEFT JOIN ALL_CONS_COLUMNS constraintColumns ON " +
				"constraints.CONSTRAINT_NAME = " +
				"constraintColumns.CONSTRAINT_NAME " +
				"LEFT JOIN ALL_CONSTRAINTS referentialConstraints ON " +
				"constraints.R_CONSTRAINT_NAME = " +
				"referentialConstraints.CONSTRAINT_NAME " +
				"LEFT JOIN ALL_CONS_COLUMNS referentialConstraintColumns ON " +
				"referentialConstraints.CONSTRAINT_NAME = " +
				"referentialConstraintColumns.CONSTRAINT_NAME";

			string where =
				$"WHERE constraints.TABLE_NAME = UPPER('{tableName}')";

			string query = $"SELECT {fields} FROM {from} {joins} {where}";

			return query;
		}

		private static string GetConstraintQueryPostgresSql(string tableName)
		{
			string fields = @"SELECT tableConstraints.constraint_type,
				tableConstraints.constraint_name,
				tableConstraints.table_name,
				constraintColumnUsage.column_name,
				constraintColumnUsageNext.table_name as referenced_table_name,
				constraintColumnUsageNext.column_name as
				referenced_column_name";

			string from =
				"information_schema.table_constraints tableConstraints";

			string joins =
				"LEFT JOIN information_schema.constraint_column_usage " +
				"constraintColumnUsage ON " +
				"tableConstraints.constraint_name= " +
				"constraintColumnUsage.constraint_name" +
				"LEFT JOIN information_schema.referential_constraints " +
				"referentialConstraints ON " +
				"tableConstraints.constraint_name = " +
				"referentialConstraints.constraint_name" +
				"LEFT JOIN information_schema.constraint_column_usage " +
				"constraintColumnUsageNext ON " +
				"referentialConstraints.unique_constraint_name = " +
				"constraintColumnUsageNext.constraint_name";

			string where =
				$"WHERE tableConstraints.table_name = '{tableName}'";

			string query = $"SELECT {fields} FROM {from} {joins} {where}";

			return query;
		}

		private static string GetConstraintQuerySqlite(string tableName)
		{
			string query = $"PRAGMA foreign_key_list('{tableName}')";

			return query;
		}

		private static string GetConstraintQuerySqlServer(string tableName)
		{
			string fields = @"SELECT tableConstraints.CONSTRAINT_TYPE,
				tableConstraints.CONSTRAINT_NAME,
				tableConstraints.TABLE_NAME,
				constraintColumnUsage.COLUMN_NAME,
				rc.UNIQUE_CONSTRAINT_NAME,
				ccu2.TABLE_NAME as REFERENCED_TABLE_NAME,
				ccu2.COLUMN_NAME as REFERENCED_COLUMN_NAME";

			string from =
				"INFORMATION_SCHEMA.TABLE_CONSTRAINTS tableConstraints";

			string joins =
				"LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE " +
				"constraintColumnUsage ON " +
				"tableConstraints.CONSTRAINT_NAME = " +
				"constraintColumnUsage.CONSTRAINT_NAME " +
				"LEFT JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS " +
				"referentialConstraints ON " +
				"tableConstraints.CONSTRAINT_NAME = " +
				"referentialConstraints.CONSTRAINT_NAME " +
				"LEFT JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE " +
				"constraintColumnUsageNext ON " +
				"referentialConstraints.UNIQUE_CONSTRAINT_NAME = " +
				"constraintColumnUsageNext.CONSTRAINT_NAME";

			string where =
				$"WHERE tableConstraints.TABLE_NAME = '{tableName}'";

			string query = $"SELECT {fields} FROM {from} {joins} {where}";

			return query;
		}

		private static void GetDependenciesRecursive(
			string key,
			Dictionary<string, Collection<string>> tableDependencies,
			Collection<string> orderedDependencies,
			HashSet<string> visited,
			HashSet<string> visiting)
		{
			if (!visited.Contains(key) && !visiting.Contains(key))
			{
				visiting.Add(key);

				if (tableDependencies.TryGetValue(
					key, out Collection<string> value))
				{
					foreach (string dependency in value)
					{
						GetDependenciesRecursive(
							dependency,
							tableDependencies,
							orderedDependencies,
							visited,
							visiting);
					}
				}

				visiting.Remove(key); // Done visiting
				visited.Add(key);     // Mark as processed
				orderedDependencies.Add(key); // Add to result (postorder)
			}
		}

		private static DataRow GetIndexConstaintsRow(
			DataTable table, DataRow row)
		{
			DataRow newRow = table.NewRow();

			newRow["ConstraintType"] = "PRIMARY KEY";
			newRow["ConstraintName"] = row["INDEX_NAME"];
			newRow["TableName"] = row["TABLE_NAME"];
			newRow["ColumnName"] = row["COLUMN_NAME"];

			return newRow;
		}

		private DataTable AddForeignKeyConstraints(
			string tableName, DataTable table)
		{
			try
			{
				string[] tableInformation = [null, null, tableName, null];

				DataTable foreignKeys = connection.GetSchema(
					"ForeignKeys", tableInformation);

				foreach (DataRow row in foreignKeys.Rows)
				{
					DataRow newRow = GetForeignKeyConstaintsRow(table, row);

					table.Rows.Add(newRow);
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is NotSupportedException ||
				exception is InvalidOperationException)
			{
				// Some providers might not support ForeignKeys schema
				Log.Error(exception.ToString());
			}
			catch (Exception exception)
			{
				Log.Error(exception.ToString());

				throw;
			}

			return table;
		}

		private DataTable AddIndexConstraints(
			string tableName, DataTable table)
		{
			try
			{
				DataTable indexes = GetIndexes(tableName);

				foreach (DataRow row in indexes.Rows)
				{
					bool exists = row["PRIMARY_KEY"] != DBNull.Value;
					bool isPrimaryKey = (bool)row["PRIMARY_KEY"];

					if (exists == true && isPrimaryKey == true)
					{
						DataRow newRow = GetIndexConstaintsRow(table, row);

						table.Rows.Add(newRow);
					}
				}
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is NotSupportedException ||
				exception is InvalidOperationException)
			{
				// Some providers might not support ForeignKeys schema
				Log.Error(exception.ToString());
			}
			catch (Exception exception)
			{
				Log.Error(exception.ToString());

				throw;
			}

			return table;
		}

		private string GetColumnName()
		{
			string columnName = "COLUMN_NAME";

			if (databaseType == DatabaseType.SQLite)
			{
				columnName = "FKEY_FROM_COLUMN";
			}

			return columnName;
		}

		private DataTable GetConstraintsByProvider(string tableName)
		{
			string query = GetConstraintQueryByProvider(tableName);

			using DbCommand command = connection.CreateCommand();
#pragma warning disable CA2100
			command.CommandText = query;
#pragma warning restore CA2100
			using var adapter =
				DbProviderFactories.GetFactory(connection).CreateDataAdapter();
			adapter.SelectCommand = command;

			DataTable constraints = new ();
			adapter.Fill(constraints);

			return constraints;
		}

		private string GetConstraintQueryByProvider(string tableName)
		{
			string query = databaseType switch
			{
				DatabaseType.MySql => GetConstraintQueryMySql(tableName),
				DatabaseType.Oracle => GetConstraintQueryOracle(tableName),
				DatabaseType.PostgresSql =>
					GetConstraintQueryPostgresSql(tableName),
				DatabaseType.SQLite => GetConstraintQuerySqlite(tableName),
				DatabaseType.SqlServer =>
					GetConstraintQuerySqlServer(tableName),

				_ => throw new NotSupportedException(
					$"Provider not supported for constraint queries")
			};

			return query;
		}

		private DataRow GetForeignKeyConstaintsRow(
			DataTable table, DataRow row)
		{
			DataRow newRow = table.NewRow();

			string columnName = GetColumnName();
			string referencedColumn = GetReferencedColumnName();
			string referencedTable = GetReferencedTableName();

			newRow["ConstraintType"] = "FOREIGN KEY";
			newRow["ConstraintName"] = row["CONSTRAINT_NAME"];
			newRow["TableName"] = row["TABLE_NAME"];
			newRow["ColumnName"] = row[columnName];
			newRow["ReferencedTable"] = row[referencedTable];
			newRow["ReferencedColumn"] = row[referencedColumn];

			return newRow;
		}

		private string GetReferencedColumnName()
		{
			string columnName = "REFERENCED_COLUMN_NAME";

			if (databaseType == DatabaseType.SQLite)
			{
				columnName = "FKEY_TO_COLUMN";
			}

			return columnName;
		}

		private string GetReferencedTableName()
		{
			string columnName = "REFERENCED_TABLE_NAME";

			if (databaseType == DatabaseType.SQLite)
			{
				columnName = "FKEY_TO_TABLE";
			}

			return columnName;
		}

		private DataTable GetSchema(string tableName, string[] restrictions)
		{
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

			DataTable schemaTable =
				connection.GetSchema(tableName, restrictions);

			connection.Close();

			return schemaTable;
		}

		private Table GetTable(DataRow row)
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
			}

			return table;
		}
	}
}
