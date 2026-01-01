/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataStoreStructure.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Data.Common;
	using System.Linq;
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
		/// Gets the SQL query statements from the provided text.
		/// </summary>
		/// <param name="queriesText">The queries text.</param>
		/// <returns>The list of queries.</returns>
		public static IReadOnlyList<string> GetSqlQueryStatements(
			string queriesText)
		{
			ArgumentNullException.ThrowIfNull(queriesText);

			char[] separator = [';'];
			string[] splitQueries = queriesText.Split(
				separator, StringSplitOptions.RemoveEmptyEntries);

			IEnumerable<string> trimmedQueries =
				splitQueries.Select(q => q.Trim());
			IEnumerable<string> nonEmptyQueries =
				trimmedQueries.Where(q => !string.IsNullOrWhiteSpace(q));

			IReadOnlyList<string> queries = [.. nonEmptyQueries];

			return queries;
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
		/// Order table.
		/// </summary>
		/// <param name="tables">The list of tables to order.</param>
		/// <returns>The ordered list of tables.</returns>
		/// <remarks>This orders the list taking dependencies into
		/// account.</remarks>
		public static Collection<Table> OrderTables(
			Collection<Table> tables)
		{
			Collection<Table> orderedTables = [];

			if (tables != null)
			{
				Dictionary<string, Collection<string>> tableDependencies = [];
				Dictionary<string, Table> tablesByName = [];

				foreach (Table table in tables)
				{
					string name = table.Name;
					tablesByName[name] = table;

					Collection<string> dependencies = [];

					foreach (ForeignKey foreignKeys in table.ForeignKeys)
					{
						dependencies.Add(foreignKeys.ChildTable);
					}

					tableDependencies.Add(name, dependencies);
				}

				Collection<string> orderedNames =
					GetOrderedDependencies(tableDependencies);

				foreach (string tableName in orderedNames)
				{
					if (tablesByName.TryGetValue(tableName, out Table table))
					{
						orderedTables.Add(table);
					}
				}
			}

			return orderedTables;
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

				column.Primary = IsPrimaryKey(row);

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
		public virtual Table GetTable(string tableName)
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
					relationship.OnDeleteAction,
					relationship.OnUpdateAction);
			}

			return foreignKey;
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
			Collection<Table> tables = new (newList);

			tables = RemoveSystemsTables(tables);

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
				// Guessing date vs. datetime based on name
				"date" => ColumnType.DateTime,
				"integer" => ColumnType.Number,
				"string" => ColumnType.String,
				"text" => ColumnType.Text,

				_ => ColumnType.Unknown,
			};

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
		protected virtual Relationship GetRelationship(DataRow foreignKey)
		{
			ArgumentNullException.ThrowIfNull(foreignKey);

			Relationship relationship = new ();

			// Using standard (or perhaps Sqlite) keys
			const string constraintNameKey = "CONSTRAINT_NAME";
			const string tableNameKey = "TABLE_NAME";
			const string columnNameKey = "FKEY_FROM_COLUMN";
			const string foreignTableNameKey = "FKEY_TO_TABLE";
			const string foreignColumnNameKey = "FKEY_TO_COLUMN";
			const string updateRuleKey = "FKEY_ON_UPDATE";
			const string deleteRuleKey = "FKEY_ON_DELETE";

			relationship.Name = foreignKey[constraintNameKey].ToString();
			relationship.ParentTable =
				foreignKey[tableNameKey].ToString();
			relationship.ParentTableCol =
				foreignKey[columnNameKey].ToString();
			relationship.ChildTable =
				foreignKey[foreignTableNameKey].ToString();
			relationship.ChildTableCol =
				foreignKey[foreignColumnNameKey].ToString();

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

			return relationship;
		}

		/// <summary>
		/// Retrieves the table associated with the specified data row.
		/// </summary>
		/// <param name="row">The data row for which to retrieve the
		/// corresponding table. Cannot be null.</param>
		/// <returns>The table that corresponds to the specified data row.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if
		/// <paramref name="row"/> is null.</exception>
		protected virtual Table GetTable(DataRow row)
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
		protected virtual bool IsPrimaryKey(DataRow column)
		{
			bool isPrimaryKey = false;

			ArgumentNullException.ThrowIfNull(column);

			if (column.Table.Columns.Contains("PRIMARY_KEY"))
			{
				string primaryKeyText = column["PRIMARY_KEY"].ToString();

				if (primaryKeyText.Equals("True", StringComparison.Ordinal))
				{
					isPrimaryKey = true;
				}
			}

			return isPrimaryKey;
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
			const string fields = "CONSTRAINT_TYPE, CONSTRAINT_NAME, " +
				"TABLE_NAME, COLUMN_NAME, REFERENCED_TABLE_NAME, " +
				"REFERENCED_COLUMN_NAME";
			const string from = "INFORMATION_SCHEMA.KEY_COLUMN_USAGE";
			const string union =
				"UNION SELECT 'PRIMARY KEY' as CONSTRAINT_TYPE, " +
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
			const string fields = @"SELECT constraints.CONSTRAINT_TYPE,
				constraints.CONSTRAINT_NAME,
				constraints.TABLE_NAME,
				constraintColumns.COLUMN_NAME,
				constraints.R_CONSTRAINT_NAME,
				referentialConstraints.TABLE_NAME as REFERENCED_TABLE_NAME,
				referentialConstraintColumns.COLUMN_NAME as
				REFERENCED_COLUMN_NAME";
			const string from = "ALL_CONSTRAINTS constraints";
			const string joins =
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
			const string fields = @"SELECT tableConstraints.constraint_type,
				tableConstraints.constraint_name,
				tableConstraints.table_name,
				constraintColumnUsage.column_name,
				constraintColumnUsageNext.table_name as referenced_table_name,
				constraintColumnUsageNext.column_name as
				referenced_column_name";

			const string from =
				"information_schema.table_constraints tableConstraints";

			const string joins =
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
			const string fields = @"SELECT tableConstraints.CONSTRAINT_TYPE,
				tableConstraints.CONSTRAINT_NAME,
				tableConstraints.TABLE_NAME,
				constraintColumnUsage.COLUMN_NAME,
				rc.UNIQUE_CONSTRAINT_NAME,
				ccu2.TABLE_NAME as REFERENCED_TABLE_NAME,
				ccu2.COLUMN_NAME as REFERENCED_COLUMN_NAME";
			const string from =
				"INFORMATION_SCHEMA.TABLE_CONSTRAINTS tableConstraints";
			const string joins =
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

		private static Collection<Table> RemoveSystemsTables(
			Collection<Table> tables)
		{
			// If more than one system table needs to be removed,
			// this should be changed to a loop that collects
			// all tables to remove first.
			foreach (Table table in tables)
			{
				if (table.Name.Equals(
					"sqlite_sequence", StringComparison.OrdinalIgnoreCase))
				{
					tables.Remove(table);
					break;
				}
			}

			return tables;
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
					"Provider not supported for constraint queries")
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
	}
}
