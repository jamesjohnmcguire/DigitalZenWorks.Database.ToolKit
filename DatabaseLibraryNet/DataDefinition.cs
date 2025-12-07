/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataDefinition.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data.Common;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Runtime.Versioning;
	using System.Text;
	using global::Common.Logging;

	/// Class <c>DataDefinition.</c>
	/// <summary>
	/// Class for support on operations on complete data storage containers.
	/// </summary>
	public static class DataDefinition
	{
		/// <summary>
		/// Diagnostics object.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// Method <c>ExportSchema.</c>
		/// <summary>
		/// Export all tables to similarly named csv files.
		/// </summary>
		/// <returns>A values indicating success or not.</returns>
		/// <param name="databaseFile">The database file to use.</param>
		/// <param name="schemaFile">The schema file to export to.</param>
		public static bool ExportSchema(
			string databaseFile, string schemaFile)
		{
			bool successCode = false;

			try
			{
				Collection<Table> tables = GetSchema(databaseFile);
				tables = OrderTables(tables);

				SqlWriter sqlWriter = new ();
				string schemaText = sqlWriter.GetTablesCreateStatements(tables);

				File.WriteAllText(schemaFile, schemaText);

				successCode = true;
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is InvalidOperationException)
			{
				string message = Strings.Exception + exception;
				Log.Error(message);
			}
			catch (Exception exception)
			{
				string message = Strings.Exception + exception;
				Log.Error(message);

				throw;
			}

			return successCode;
		}

		/// Method <c>ExportSchemaOleDb.</c>
		/// <summary>
		/// Export all tables to similarly named csv files.
		/// </summary>
		/// <returns>A values indicating success or not.</returns>
		/// <param name="databaseFile">The database file to use.</param>
		/// <param name="schemaFile">The schema file to export to.</param>
#if NET5_0_OR_GREATER
		[SupportedOSPlatform("windows")]
#endif
		public static bool ExportSchemaOleDb(
			string databaseFile, string schemaFile)
		{
			bool successCode = false;

			try
			{
				Collection<Table> tables = GetSchemaOleDb(databaseFile);
				tables = OrderTables(tables);

				SqlWriterOleDb sqlWriter = new ();
				string schemaText = sqlWriter.GetTablesCreateStatements(tables);

				File.WriteAllText(schemaFile, schemaText);

				successCode = true;
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is InvalidOperationException)
			{
				string message = Strings.Exception + exception;
				Log.Error(message);
			}
			catch (Exception exception)
			{
				string message = Strings.Exception + exception;
				Log.Error(message);

				throw;
			}

			return successCode;
		}

		/// <summary>
		/// GetColumnInfo - returns details of a column statement.
		/// </summary>
		/// <param name="dataDefinition">The data definition to use.</param>
		/// <returns>The details of the column.</returns>
		public static string GetColumnInfo(string dataDefinition)
		{
			string columnsInfo = null;

			if (!string.IsNullOrWhiteSpace(dataDefinition))
			{
#if NETCOREAPP1_0_OR_GREATER
				const char check = '(';
#else
				const string check = "(";
#endif

				int splitIndex = dataDefinition.IndexOf(
					check,
					StringComparison.Ordinal);

				splitIndex++;
#if NETCOREAPP1_0_OR_GREATER
				columnsInfo = dataDefinition[splitIndex..];
#else
				columnsInfo = dataDefinition.Substring(splitIndex);
#endif
			}

			return columnsInfo;
		}

		/// <summary>
		/// Returns the column type.
		/// </summary>
		/// <param name="column">The column to use.</param>
		/// <returns>The column type.</returns>
		public static ColumnType GetColumnType(
			string column)
		{
			string[] columnTypeComareKeys =
			[
				"AUTONUMBER", "IDENTITY",
				"AUTOINCREMENT", "BIGINT", "LONGVARBINARY", "LONGVARCHAR",
				"VARBINARY", "VARCHAR", "BINARY", "BIT", "LONGBLOB",
				"MEDIUMBLOB", "BLOB", "BOOLEAN", "BYTE", "NVARCHAR", "CHAR",
				"CURRENCY", "CURSOR", "SMALLDATETIME", "SMALLINT",
				"SMALLMONEY", "DATETIME2", "DATETIMEOFFSET", "DATETIME",
				"DATE", "DECIMAL", "DOUBLE", "HYPERLINK", "ENUM", "FLOAT",
				"IMAGE", "INTEGER", "MEDIUMINT", "TINYINT", "INT",
				"JAVAOBJECT", "LONGTEXT", "LONG", "LOOKUPWIZARD", "MEDIUMTEXT",
				"MEMO", "MONEY", "NCHAR", "NTEXT", "NUMBER", "NUMERIC",
				"OLEOBJECT", "OLE", "REAL", "SET", "SINGLE", "SQLVARIANT",
				"STRING", "TABLE", "TINYTEXT", "TEXT", "TIMESTAMP", "TIME",
				"UNIQUEIDENTIFIER", "XML", "YEAR", "YESNO"
			];

			ColumnType[] types =
			[
				ColumnType.AutoNumber, ColumnType.Identity,
				ColumnType.Identity, ColumnType.BigInt,
				ColumnType.LongVarBinary, ColumnType.LongVarChar,
				ColumnType.VarBinary, ColumnType.VarChar, ColumnType.Binary,
				ColumnType.Bit, ColumnType.LongBlob, ColumnType.MediumBlob,
				ColumnType.Blob, ColumnType.Boolean, ColumnType.Byte,
				ColumnType.NVarChar, ColumnType.Char, ColumnType.Currency,
				ColumnType.Cursor, ColumnType.SmallDateTime,
				ColumnType.SmallInt, ColumnType.SmallMoney,
				ColumnType.DateTime2, ColumnType.DateTimeOffset,
				ColumnType.DateTime, ColumnType.Date, ColumnType.Decimal,
				ColumnType.Double, ColumnType.Hyperlink, ColumnType.Enum,
				ColumnType.Float, ColumnType.Image, ColumnType.Integer,
				ColumnType.MediumInt, ColumnType.TinyInt, ColumnType.Int,
				ColumnType.JavaObject, ColumnType.LongText, ColumnType.Long,
				ColumnType.LookupWizard, ColumnType.MediumText,
				ColumnType.Memo, ColumnType.Money, ColumnType.NChar,
				ColumnType.NText, ColumnType.Number, ColumnType.Numeric,
				ColumnType.OleObject, ColumnType.Ole, ColumnType.Real,
				ColumnType.Set, ColumnType.Single, ColumnType.SqlVariant,
				ColumnType.String, ColumnType.Table, ColumnType.TinyText,
				ColumnType.Text, ColumnType.Timestamp, ColumnType.Time,
				ColumnType.UniqueIdentifier, ColumnType.Xml, ColumnType.Year,
				ColumnType.Boolean
			];

			ColumnType columnType = ColumnType.Other;

			if (column != null)
			{
				for (int index = 0; index < columnTypeComareKeys.Length;
					index++)
				{
					if (CompareColumnType(
						column,
						columnTypeComareKeys[index],
						types[index],
						ref columnType))
					{
						break;
					}
				}
			}

			if (columnType == ColumnType.Other)
			{
				string message = Strings.WarningOther + column;
				Log.Warn(message);
			}

			return columnType;
		}

		/// <summary>
		/// Gets the foreign key relationships.
		/// </summary>
		/// <param name="relationships">A set of relationships.</param>
		/// <returns>The foreign key relationships.</returns>
		public static ForeignKey[] GetForeignKeyRelationships(
			Relationship[] relationships)
		{
			ForeignKey[] keys =
				DataStoreStructure.GetForeignKeyRelationships(relationships);

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
		/// Retrieves the schema information for all tables in the specified
		/// database file.
		/// </summary>
		/// <param name="databaseFile">The path to the database file from
		/// which to retrieve schema information. Must refer to a valid and
		/// accessible database file.</param>
		/// <returns>A collection of <see cref="Table"/> objects representing
		/// the tables defined in the database. The collection will be empty
		/// if no tables are found.</returns>
		public static Collection<Table> GetSchema(string databaseFile)
		{
			DatabaseType databaseType = GetDatabaseType(databaseFile);

			using DataStoreStructure schema = new (databaseType, databaseFile);
			Collection<Table> tables = schema.GetSchema();

			return tables;
		}

		/// <summary>
		/// Retrieves the schema information for all tables in the specified
		/// database file.
		/// </summary>
		/// <param name="databaseFile">The path to the database file from
		/// which to retrieve schema information. Must refer to a valid and
		/// accessible database file.</param>
		/// <returns>A collection of <see cref="Table"/> objects representing
		/// the tables defined in the database. The collection will be empty
		/// if no tables are found.</returns>
#if NET5_0_OR_GREATER
		[SupportedOSPlatform("windows")]
#endif
		public static Collection<Table> GetSchemaOleDb(string databaseFile)
		{
			using OleDbSchema oleDbSchema = new (databaseFile);
			Collection<Table> tables = oleDbSchema.GetSchema();

			return tables;
		}

		/// <summary>
		/// GetTableDefinitions - returns an array of table definitions.
		/// </summary>
		/// <param name="tableDefinitionsFile">The table definitions file.</param>
		/// <returns>An array of table definitions.</returns>
		public static string[] GetTableDefinitions(
			string tableDefinitionsFile)
		{
			string[] queries = null;

			if (!string.IsNullOrWhiteSpace(tableDefinitionsFile))
			{
				string[] stringSeparators = ["\n\n", "\r\n\r\n"];
				queries = tableDefinitionsFile.Split(
					stringSeparators,
					64000,
					StringSplitOptions.RemoveEmptyEntries);
			}

			return queries;
		}

		/// <summary>
		/// GetTableName - returns the name of the table.
		/// </summary>
		/// <param name="dataDefinition">The data definition.</param>
		/// <returns>The name of the table.</returns>
		public static string GetTableName(string dataDefinition)
		{
			string tableName = null;

			if (!string.IsNullOrWhiteSpace(dataDefinition))
			{
				const char separators = '(';
				string[] tableParts = dataDefinition.Split(separators);

				string[] tableNameParts = tableParts[0].Split(['[', ']', '`']);

				if (tableNameParts.Length > 1)
				{
					tableName = tableNameParts[1];
				}
			}

			return tableName;
		}

		/// <summary>
		/// Creates a file with the given schema.
		/// </summary>
		/// <param name="schemaFile">The schema file.</param>
		/// <param name="databaseFile">The database file.</param>
		/// <returns>A values indicating success or not.</returns>
		public static bool ImportSchema(string schemaFile, string databaseFile)
		{
			bool successCode = false;

			if (!File.Exists(schemaFile))
			{
				string message = $"Schema file not found: {schemaFile}";
				throw new FileNotFoundException(message, schemaFile);
			}
			else
			{
				try
				{
					string fileContents = File.ReadAllText(schemaFile);

					IReadOnlyList<string> queries =
						GetSqlQueryStatements(fileContents);
					DatabaseType databaseType = GetDatabaseType(databaseFile);

					string connectionString = DataStorage.GetConnectionString(
						databaseType, databaseFile);

					using DataStorage database =
						new(databaseType, connectionString);

					successCode = ExecuteQueries(database, queries);
				}
				catch (Exception exception) when
					(exception is ArgumentNullException ||
					exception is ArgumentException ||
					exception is FileNotFoundException ||
					exception is DirectoryNotFoundException ||
					exception is IOException ||
					exception is OutOfMemoryException ||
					exception is DbException)
				{
					string message = Strings.Exception + exception;
					Log.Error(message);
				}
				catch (Exception exception)
				{
					string message = Strings.Exception + exception;
					Log.Error(message);

					throw;
				}
			}

			return successCode;
		}

		/// <summary>
		/// Creates a file with the given schema.
		/// </summary>
		/// <param name="schemaFile">The schema file.</param>
		/// <param name="databaseFile">The database file.</param>
		/// <returns>A values indicating success or not.</returns>
#if NET5_0_OR_GREATER
		[SupportedOSPlatform("windows")]
#endif
		public static bool ImportSchemaOleDb(
			string schemaFile, string databaseFile)
		{
			bool successCode = false;

			if (!File.Exists(schemaFile))
			{
				string message = $"Schema file not found: {schemaFile}";
				throw new FileNotFoundException(message, schemaFile);
			}
			else
			{
				try
				{
					string fileContents = File.ReadAllText(schemaFile);
					IReadOnlyList<string> queries =
						GetSqlQueryStatements(fileContents);

					string extension = Path.GetExtension(databaseFile);

					if (extension.Equals(
							".mdb", StringComparison.OrdinalIgnoreCase) ||
						extension.Equals(
							".accdb", StringComparison.OrdinalIgnoreCase))
					{
						const string provider = "Microsoft.ACE.OLEDB.12.0";
						string connectionString =
							OleDbHelper.BuildConnectionString(
								databaseFile, provider);

						successCode = ImportSchemaMdb(queries, databaseFile);
					}
					else
					{
						string message =
							$"Unsupported database file extension: {extension}";
						throw new ArgumentException(
							message, nameof(databaseFile));
					}
				}
				catch (Exception exception) when
					(exception is ArgumentNullException ||
					exception is ArgumentException ||
					exception is FileNotFoundException ||
					exception is DirectoryNotFoundException ||
					exception is IOException ||
					exception is OutOfMemoryException ||
					exception is System.Data.OleDb.OleDbException)
				{
					string message = Strings.Exception + exception;
					Log.Error(message);
				}
				catch (Exception exception)
				{
					string message = Strings.Exception + exception;
					Log.Error(message);

					throw;
				}
			}

			return successCode;
		}

		/// <summary>
		/// Checks to see if the given field is a time related field.
		/// </summary>
		/// <param name="field">The field to check.</param>
		/// <returns>Whether the field is a time related field.</returns>
		public static bool IsTimeField(string field)
		{
			bool returnCode = false;

			if (!string.IsNullOrWhiteSpace(field))
			{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				if (field.Contains("time", StringComparison.OrdinalIgnoreCase))
#else
				if (field.Contains("time"))
#endif
				{
					returnCode = true;
				}
			}

			return returnCode;
		}

		/// <summary>
		/// Order table.
		/// </summary>
		/// <param name="tables">The list of tables to order.</param>
		/// <returns>The ordered list of tables.</returns>
		/// <remarks>This orders the list taking dependencies into
		/// account.</remarks>
		public static Collection<string> OrderTable(
			Collection<Table> tables)
		{
			Collection<string> orderedTableNames = [];

			Collection<Table> orderedTables = OrderTables(tables);

			foreach (Table table in orderedTables)
			{
				orderedTableNames.Add(table.Name);
			}

			return orderedTableNames;
		}

		/// <summary>
		/// Order tables.
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

		private static bool CompareColumnType(
			string column,
			string nameCheck,
			ColumnType columnType,
			ref ColumnType columnTypeOut)
		{
			bool found = false;

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			if (column.ToUpperInvariant().Contains(
					nameCheck, StringComparison.OrdinalIgnoreCase) ||
				column.ToUpperInvariant().Equals(
					nameCheck, StringComparison.Ordinal))
#else
			if (column.ToUpperInvariant().Contains(nameCheck) ||
				column.ToUpperInvariant().Equals(
					nameCheck, StringComparison.Ordinal))
#endif
			{
				columnTypeOut = columnType;
				found = true;
			}

			return found;
		}

		private static bool ExecuteQueries(
			DataStorage database, IReadOnlyList<string> queries)
		{
			bool result = false;

			ArgumentNullException.ThrowIfNull(database);

			if (queries != null)
			{
				foreach (string sqlQuery in queries)
				{
					try
					{
						string message = Strings.Command + sqlQuery;
						Log.Info(message);

						database.ExecuteNonQuery(sqlQuery);
					}
					catch (Exception exception) when
						(exception is ArgumentNullException ||
						exception is OutOfMemoryException ||
						exception is DbException ||
						exception is System.Data.OleDb.OleDbException)
					{
						string message = Strings.Exception + exception;
						Log.Error(message);
					}
					catch (Exception exception)
					{
						string message = Strings.Exception + exception;
						Log.Error(message);

						throw;
					}
				}

				result = true;
			}

			return result;
		}

		private static byte[] GetDatabaseFileHeaderBytes(string databaseFile)
		{
			byte[] header = null;

			try
			{
				using FileStream stream = new (
					databaseFile,
					FileMode.Open,
					FileAccess.Read,
					FileShare.ReadWrite);

				// Read enough bytes to check all signatures
				header = new byte[32];
				int bytesRead = stream.Read(header, 0, header.Length);
			}
			catch (Exception exception) when
				(exception is ArgumentNullException)
			{
				Log.Error(Strings.Exception + exception);
			}

			return header;
		}

		private static DatabaseType GetDatabaseType(string databaseFile)
		{
			DatabaseType databaseType = DatabaseType.Unknown;

			bool exists = File.Exists(databaseFile);

			if (exists == true)
			{
				byte[] header = GetDatabaseFileHeaderBytes(databaseFile);
				databaseType = GetDatabaseTypeByFileHeaderBytes(header);

				// Fallback to extension-based detection
				if (databaseType == DatabaseType.Unknown)
				{
					databaseType = GetDatabaseTypeByExtension(databaseFile);
				}
			}

			return databaseType;
		}

		private static DatabaseType GetDatabaseTypeByFileHeaderBytes(
			byte[] header)
		{
			DatabaseType databaseType = DatabaseType.Unknown;

			if (header == null || header.Length < 4)
			{
				databaseType = DatabaseType.Unknown;
			}
			else if (header.Length >= 16 &&
				header[0] == 0x53 && header[1] == 0x51 &&
				header[2] == 0x4C && header[3] == 0x69 &&
				header[4] == 0x74 && header[5] == 0x65 &&
				header[6] == 0x20 && header[7] == 0x66 &&
				header[8] == 0x6F && header[9] == 0x72 &&
				header[10] == 0x6D && header[11] == 0x61 &&
				header[12] == 0x74 && header[13] == 0x20 &&
				header[14] == 0x33)
			{
				// SQLite: "SQLite format 3\0" (starts at byte 0)
				databaseType = DatabaseType.SQLite;
			}
			else if (header.Length >= 20 &&
				header[0] == 0x00 && header[1] == 0x01 &&
				header[2] == 0x00 && header[3] == 0x00 &&
				header[4] == 0x53 && header[5] == 0x74 &&
				header[6] == 0x61 && header[7] == 0x6E &&
				header[8] == 0x64 && header[9] == 0x61 &&
				header[10] == 0x72 && header[11] == 0x64 &&
				header[12] == 0x20 && header[13] == 0x41 &&
				header[14] == 0x43 && header[15] == 0x45)
			{
				// MS Access 2007+ (.accdb): 0x00, 0x01, 0x00, 0x00,
				// "Standard ACE DB"
				databaseType = DatabaseType.OleDb;
			}
			else if (header.Length >= 20 &&
				header[0] == 0x00 && header[1] == 0x01 &&
				header[2] == 0x00 && header[3] == 0x00 &&
				header[4] == 0x53 && header[5] == 0x74 &&
				header[6] == 0x61 && header[7] == 0x6E &&
				header[8] == 0x64 && header[9] == 0x61 &&
				header[10] == 0x72 && header[11] == 0x64 &&
				header[12] == 0x20 && header[13] == 0x4A &&
				header[14] == 0x65 && header[15] == 0x74)
			{
				// MS Access 97-2003 (.mdb): 0x00, 0x01, 0x00, 0x00,
				// "Standard Jet DB"
				databaseType = DatabaseType.OleDb;
			}
			else if (header[0] == 0x01 && header[1] == 0x0F &&
				header[2] == 0x00 && header[3] == 0x00)
			{
				// SQL Server .mdf: 0x01, 0x0F, 0x00, 0x00
				// (page header signature)
				databaseType = DatabaseType.SqlServer;
			}

			// Firebird/Interbase: Check for specific page size markers
			// This is trickier and might need extension fallback
			return databaseType;
		}

		private static DatabaseType GetDatabaseTypeByExtension(
			string databaseFile)
		{
			string extension = Path.GetExtension(databaseFile);
#pragma warning disable CA1308
			extension = extension.ToLowerInvariant();
#pragma warning restore CA1308

			DatabaseType databaseType = extension switch
			{
				".db" or ".sqlite" or ".sqlite3" or ".db3" or ".sdb" =>
					DatabaseType.SQLite,
				".mdb" or ".accdb" => DatabaseType.OleDb,
				".mdf" => DatabaseType.SqlServer,
				_ => DatabaseType.Unknown
			};

			return databaseType;
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

		private static IReadOnlyList<string> GetSqlQueryStatements(
			string queriesText)
		{
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

#if NET5_0_OR_GREATER
		[SupportedOSPlatform("windows")]
#endif
		private static bool ImportSchemaMdb(
			IReadOnlyList<string> queries, string databaseFile)
		{
			string connectionString =
				OleDbHelper.BuildConnectionString(databaseFile);

			using DataStorage database =
				new (DatabaseType.OleDb, connectionString);

			bool successCode = OleDbHelper.ExecuteQueries(database, queries);

			return successCode;
		}
	}
}
