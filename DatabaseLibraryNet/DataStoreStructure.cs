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
	using System.Data.SQLite;
	using System.Globalization;
	using global::Common.Logging;
	using Microsoft.Data.SqlClient;
	using MySql.Data.MySqlClient;

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
		private DbConnection connection;

		/// <summary>
		/// Initializes a new instance of the <see cref="DataStoreStructure"/> class.
		/// </summary>
		/// <param name="databaseType">The database type.</param>
		/// <param name="databaseFile">The database file to use.</param>
		public DataStoreStructure(DatabaseType databaseType, string databaseFile)
		{
			this.databaseType = databaseType;

			string baseFormat = "Data Source={0}";

			string connectionString = string.Format(
				CultureInfo.InvariantCulture,
				baseFormat,
				databaseFile);

			connection = GetConnection(databaseType, connectionString);
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
				connection.Open();

				DataTable schemaTable = connection.GetSchema("Tables");

				connection.Close();

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

				string dataType = row["DATA_TYPE"].ToString();

				switch (dataType)
				{
					case "integer":
						column.ColumnType = ColumnType.Number;
						break;
					case "string":
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
					case "date":
						// Guessing date vs. datetime based on name
						column.ColumnType = ColumnType.DateTime;
						break;
					default:
						column.ColumnType = ColumnType.Unknown;
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
		public static ForeignKey GetForeignKeyRelationship(
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
				foreignKey = new (
					relationship.Name,
					relationship.ChildTableCol,
					relationship.ParentTable,
					relationship.ParentTableCol,
					relationship.OnDeleteCascade,
					relationship.OnUpdateCascade);
			}

			return foreignKey;
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
			connection.Open();

			DataTable constraints = GetBaseConstraints();

			constraints = AddForeignKeyConstraints(
				tableName, constraints);

			constraints = AddIndexConstraints(
				tableName, constraints);

			connection.Close();

			return constraints;
		}

		/// <summary>
		/// Gets the foreign keys from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetForeignKeys(string tableName)
		{
			connection.Open();

			string[] testTable = [null, null, tableName];

			DataTable schemaTable =
				connection.GetSchema("ForeignKeys", testTable);

			connection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Gets the constraints from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetIndexes(string tableName)
		{
			if (connection.State != ConnectionState.Open)
			{
				connection.Open();
			}

			string[] tableInformation = [null, null, tableName];

			DataTable schemaTable =
				connection.GetSchema("Indexes", tableInformation);

			connection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Gets the primary keys from the given table.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public DataTable GetPrimaryKeys(string tableName)
		{
			connection.Open();

			string[] tableInformation = [null, null, tableName];

			DataTable schemaTable = connection.GetSchema(
				"PrimaryKeys", tableInformation);

			connection.Close();

			return schemaTable;
		}

		/// <summary>
		/// Gets a list of relationships.
		/// </summary>
		/// <param name="tableName">The table name.</param>
		/// <returns>A list of relationships.</returns>
		public Collection<Relationship> GetRelationships(string tableName)
		{
			Collection<Relationship> relationships = [];

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
		/// <param name="databaseFile">The path to the database file for which
		/// to retrieve schema information. Must refer to a valid and
		/// accessible database file.</param>
		/// <returns>A collection of <see cref="Table"/> objects representing
		/// the tables in the database, each populated with its foreign key
		/// relationships.</returns>
		public Collection<Table> GetSchema(string databaseFile)
		{
			Dictionary<string, Table> tableDictionary = [];
			List<Relationship> relationships = [];

			foreach (DataRow row in TableNames.Rows)
			{
				object nameRaw = row["TABLE_NAME"];
				string tableName = nameRaw.ToString();

				Table table = GetTable(tableName);

				Collection<Relationship> newRelationships =
					GetRelationships(tableName);
				relationships = [.. relationships, .. newRelationships];

				tableDictionary.Add(tableName, table);
			}

			// Add foreign keys to table, using relationships
			foreach (Relationship relationship in relationships)
			{
				string name = relationship.ChildTable;

				ForeignKey foreignKey =
					DataStoreStructure.GetForeignKeyRelationship(relationship);

				Table table = tableDictionary[name];

				table.ForeignKeys.Add(foreignKey);
			}

			List<Table> newList = [.. tableDictionary.Values];
			Collection<Table> tables = new (newList);

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
			connection.Open();

			string[] testTable = [null, null, tableName];

			DataTable schemaTable = connection.GetSchema("Columns", testTable);

			connection.Close();

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
				connection?.Close();
				connection = null;
			}
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

		private static DataTable GetBaseConstraints()
		{
			DataTable table = new ();

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

		private static Relationship GetRelationship(DataRow foreignKey)
		{
			Relationship relationship = new ();
			relationship.Name = foreignKey["FK_NAME"].ToString();
			relationship.ParentTable =
				foreignKey["PK_TABLE_NAME"].ToString();
			relationship.ParentTableCol =
				foreignKey["PK_COLUMN_NAME"].ToString();
			relationship.ChildTable =
				foreignKey["FK_TABLE_NAME"].ToString();
			relationship.ChildTableCol =
				foreignKey["FK_COLUMN_NAME"].ToString();

			if (foreignKey["UPDATE_RULE"].ToString() != "NO ACTION")
			{
				relationship.OnUpdateCascade = true;
			}

			if (foreignKey["DELETE_RULE"].ToString() != "NO ACTION")
			{
				relationship.OnDeleteCascade = true;
			}

			return relationship;
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

		private DbConnection GetConnection(
			DatabaseType databaseType, string connectionText)
		{
			switch (databaseType)
			{
				case DatabaseType.MySql:
					MySqlConnection mySqlConnection = new (connectionText);
					connection = mySqlConnection;
					break;
				case DatabaseType.SQLite:
					SQLiteConnection sqliteConnection = new (connectionText);
					connection = sqliteConnection;
					break;
				case DatabaseType.SqlServer:
					SqlConnection sqlConnection = new (connectionText);
					connection = sqlConnection;
					break;
				case DatabaseType.Unknown:
					break;
				case DatabaseType.Oracle:
					break;
				default:
					break;
			}

			return connection;
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
	}
}
