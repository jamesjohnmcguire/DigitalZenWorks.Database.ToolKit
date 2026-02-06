/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriter.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;

	/// <summary>
	/// SQL writer helper class.
	/// </summary>
	public class SqlWriter
	{
		private readonly string[] rowColumnValues;

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlWriter"/> class.
		/// </summary>
		public SqlWriter()
		{
			rowColumnValues = Array.Empty<string>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SqlWriter"/> class
		/// using the specified array of row and column values.
		/// </summary>
		/// <param name="rowColumnValues">An array of strings containing the
		/// values to be written for each row and column. Cannot be null.
		/// </param>
		public SqlWriter(string[] rowColumnValues)
		{
			this.rowColumnValues = rowColumnValues;
		}

		/// <summary>
		/// Create Insert Statement.
		/// </summary>
		/// <param name="tableName">The table name.</param>
		/// <param name="keyPairs">The set of key pairs.</param>
		/// <returns>A SQL insert statement.</returns>
		public static string? CreateInsertStatement(
			string tableName, IList<KeyValuePair<string, string>> keyPairs)
		{
			string columns = string.Empty;
			string values = string.Empty;

			if (keyPairs != null)
			{
				int count = keyPairs.Count;

				for (int index = 0; index < count; index++)
				{
					KeyValuePair<string, string> keyPair = keyPairs[index];

					columns += keyPair.Key;
					values += keyPair.Value;

					if (index < count - 1)
					{
						columns += ", ";
						values += ", ";
					}
				}
			}

			string? insertStatement = string.Format(
				CultureInfo.InvariantCulture,
				"INSERT INTO {0} ({1}) VALUES ({2});",
				tableName,
				columns,
				values);

			return insertStatement;
		}

		/// <summary>
		/// Create Insert Statement.
		/// </summary>
		/// <param name="tableName">The table name.</param>
		/// <param name="primaryKeyPairs">The set of primary
		/// key pairs.</param>
		/// <param name="keyPairs">The set of key pairs.</param>
		/// <returns>A SQL insert statement.</returns>
		public static string? CreateUpdateStatement(
			string tableName,
			IList<KeyValuePair<string, string>> primaryKeyPairs,
			IList<KeyValuePair<string, string>> keyPairs)
		{
			string? statement = null;

			if (primaryKeyPairs != null && keyPairs != null)
			{
				string pairs = string.Empty;
				string where = "WHERE ";

				int count = keyPairs.Count;

				for (int index = 0; index < count; index++)
				{
					KeyValuePair<string, string> keyPair = keyPairs[index];

					string pair = keyPair.Key + " = " + keyPair.Value;

					if (index < count - 1)
					{
						pair += ", ";
					}

					pairs += pair;
				}

				count = primaryKeyPairs.Count;

				for (int index = 0; index < count; index++)
				{
					KeyValuePair<string, string> keyPair =
						primaryKeyPairs[index];

					string pair = string.Empty;

					if (index > 0)
					{
						pair += " AND ";
					}

					pair += keyPair.Key + " = " + keyPair.Value;

					where += pair;
				}

				statement = string.Format(
					CultureInfo.InvariantCulture,
					"UPDATE {0} SET {1} {2};",
					tableName,
					pairs,
					where);
			}

			return statement;
		}

		/// <summary>
		/// Get OA date.
		/// </summary>
		/// <param name="rawValue">The raw value to check.</param>
		/// <returns>A text representation of the date.</returns>
		public static string GetOADate(double rawValue)
		{
			DateTime dateTime = DateTime.FromOADate(rawValue);
			string? date = dateTime.ToString(
				"yyyy-MM-dd", CultureInfo.InvariantCulture);

			return date;
		}

		/// <summary>
		/// Escape string.
		/// </summary>
		/// <param name="dataItem">The value to escape.</param>
		/// <returns>The escaped value.</returns>
		public static string? EscapeString(string dataItem)
		{
			if (dataItem != null)
			{
				dataItem = dataItem.Trim();
				dataItem = "'" + dataItem + "'";
			}

			return dataItem;
		}

		/// <summary>
		/// Get escaped value.
		/// </summary>
		/// <param name="dataItem">The value to escape.</param>
		/// <returns>The escaped value.</returns>
		public static string? GetEscapedValue(string dataItem)
		{
			if (dataItem != null)
			{
				bool enclosed = false;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
				bool beginEscape = dataItem.StartsWith('\'');
				bool endsEscape = dataItem.EndsWith('\'');
#else
				bool beginEscape = dataItem.StartsWith("\'");
				bool endsEscape = dataItem.EndsWith("\'");
#endif

				if (beginEscape == true && endsEscape == true)
				{
					enclosed = true;

					dataItem = dataItem.Trim('\'');
				}

				Regex unescapedApostropheRegex = new(@"(?<!')'(?!')");

				bool isUnescaped = unescapedApostropheRegex.IsMatch(dataItem);

				if (isUnescaped == true)
				{
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
					dataItem = dataItem.Replace(
						"'", "''", StringComparison.Ordinal);
#else
					dataItem = dataItem.Replace(
						"'", "''");
#endif
				}

				if (enclosed == true)
				{
					dataItem = '\'' + dataItem + '\'';
				}
			}

			return dataItem;
		}

		/// <summary>
		/// Get escaped value.
		/// </summary>
		/// <param name="columnValues">The column values.</param>
		/// <param name="column">the column index.</param>
		/// <returns>The escaped value.</returns>
		public static string? GetEscapedValue(
			string[] columnValues, int column)
		{
			string? dataItem = null;

			if (columnValues != null)
			{
				dataItem = columnValues[column];
				dataItem = GetEscapedValue(dataItem);
			}

			return dataItem;
		}

		/// <summary>
		/// Get SQL file name.
		/// </summary>
		/// <param name="sqlFile">The raw file name.</param>
		/// <param name="defaultName">The default file name.</param>
		/// <returns>The SQL file name.</returns>
		public static string GetSqlFileName(string sqlFile, string defaultName)
		{
			string? outputFile;

			if (!string.IsNullOrWhiteSpace(sqlFile))
			{
				outputFile = sqlFile;
			}
			else
			{
				outputFile = Path.ChangeExtension(defaultName, ".sql");
			}

			outputFile = Path.GetFullPath(outputFile);

			return outputFile;
		}

		/// <summary>
		/// Get SQL nuLL value.
		/// </summary>
		/// <param name="dataItem">The data item to check.</param>
		/// <returns>The SQL value with NULL as replacement for a null or
		/// empty value.</returns>
		public static string GetSqlNullValue(string? dataItem)
		{
			if (string.IsNullOrWhiteSpace(dataItem))
			{
				dataItem = "NULL";
			}

			return dataItem;
		}

		/// <summary>
		/// Get SQL value.
		/// </summary>
		/// <param name="columnValues">The column values.</param>
		/// <param name="column">the column index.</param>
		/// <param name="escapeNonNull">A value indicating whether to escape
		/// non-null values.</param>
		/// <param name="defaultNull">A value indicating whether null is the
		/// default value.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The SQL value.</returns>
		public static string? GetSqlValue(
			string[] columnValues,
			int column,
			bool escapeNonNull = true,
			bool defaultNull = true,
			string? defaultValue = null)
		{
			string? dataItem = null;

			if (columnValues != null)
			{
				dataItem = columnValues[column];

				if (dataItem != null)
				{
					dataItem = dataItem.Trim();

					// Replace extra spaces.
					dataItem = Regex.Replace(dataItem, @"\s+", " ");

					dataItem = GetEscapedValue(dataItem);
				}

				if (defaultNull == true)
				{
					dataItem = GetSqlNullValue(dataItem);
				}
				else if (string.IsNullOrWhiteSpace(dataItem) &&
					defaultValue != null)
				{
					dataItem = defaultValue;
				}

				if (escapeNonNull == true &&
					dataItem != null &&
					!dataItem.Equals("NULL", StringComparison.Ordinal))
				{
					dataItem = EscapeString(dataItem);
				}
			}

			return dataItem;
		}

		/// <summary>
		/// Is null or SQL NULL check.
		/// </summary>
		/// <param name="checkText">The text to check.</param>
		/// <returns>A value indicating whether the text is null or
		/// a SQL NULL value.</returns>
		public static bool IsNullOrSqlNull(string checkText)
		{
			bool isNullOrWhiteSpaceOrSqlNull = false;

			if (checkText == null ||
				checkText.Equals("NULL", StringComparison.Ordinal))
			{
				isNullOrWhiteSpaceOrSqlNull = true;
			}

			return isNullOrWhiteSpaceOrSqlNull;
		}

		/// <summary>
		/// Write statements.
		/// </summary>
		/// <param name="sqlFile">The SQL file to write to.</param>
		/// <param name="buffers">The list of buffers to write out.</param>
		public static void WriteStatements(
			string sqlFile, IList<IList<string>> buffers)
		{
			if (buffers != null)
			{
				StringBuilder builder = new();

				foreach (IList<string> buffer in buffers)
				{
					foreach (string statement in buffer)
					{
						builder.AppendLine(statement);
					}

					builder.AppendLine();
				}

				string statements = builder.ToString();

				File.WriteAllText(sqlFile, statements);
			}
		}

		/// <summary>
		/// Get date.
		/// </summary>
		/// <param name="column">The column index to check.</param>
		/// <returns>A text representation of the date.</returns>
		public string? GetDate(int column)
		{
			string? date = "NULL";

			string? rawValue = GetSqlValue(column, false, false);

			if (string.IsNullOrWhiteSpace(rawValue))
			{
				date = GetSqlNullValue(rawValue);
			}
			else
			{
				bool result = double.TryParse(rawValue, out double outDate);

				if (result == true)
				{
					date = GetOADate(outDate);
					date = "'" + date + "'";
				}
			}

			return date;
		}

		/// <summary>
		/// Get SQL value.
		/// </summary>
		/// <param name="column">the column index.</param>
		/// <param name="escapeNonNull">A value indicating whether to escape
		/// non-null values.</param>
		/// <param name="defaultNull">A value indicating whether null is the
		/// default value.</param>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The SQL value.</returns>
		public string? GetSqlValue(
			int column,
			bool escapeNonNull = true,
			bool defaultNull = true,
			string? defaultValue = null)
		{
			string? dataItem = GetSqlValue(
				rowColumnValues,
				column,
				escapeNonNull,
				defaultNull,
				defaultValue);

			return dataItem;
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
		/// <param name="isLast">A value indicating whether this is the last
		/// table.</param>
		/// <returns>A string containing the SQL CREATE TABLE statement that
		/// defines the table, its columns, primary key, and foreign key
		/// constraints.</returns>
		public virtual string GetTableCreateStatement(
			Table table, bool isLast = false)
		{
#if NET6_0_OR_GREATER
			ArgumentNullException.ThrowIfNull(table);
#else
			if (table == null)
			{
				throw new ArgumentNullException(nameof(table));
			}
#endif

			Collection<ForeignKey> foreignKeys = table.ForeignKeys;

			string sql = string.Format(
				CultureInfo.InvariantCulture,
				"CREATE TABLE \"{0}\"{1}({1}",
				table.Name,
				Environment.NewLine);

			SortedList<int, Column> columns = GetOrdinalSortedColumns(table);

			for (int index = 0; index < columns.Count; index++)
			{
				Column column = columns.Values[index];

				bool isLastColumn = false;

				if (index == columns.Count - 1 && foreignKeys.Count == 0)
				{
					isLastColumn = true;
				}

				sql += GetColumnSql(column, isLastColumn);
			}

			// These seem to be in reverse order.
			for (int index = foreignKeys.Count - 1; index >= 0; index--)
			{
				ForeignKey foreignKey = foreignKeys[index];

				bool isLastKey = false;

				if (index == 0)
				{
					isLastKey = true;
				}

				sql += GetForeignKeySql(foreignKey, isLastKey);
			}

			if (foreignKeys.Count > 0)
			{
				sql += Environment.NewLine;
			}

			sql += ");";

			if (isLast == false)
			{
				sql += Environment.NewLine;
			}

			return sql;
		}

		/// <summary>
		/// Generates SQL CREATE TABLE statements for the specified collection
		/// of tables.
		/// </summary>
		/// <param name="tables">A collection of <see cref="Table"/> objects for
		/// which to generate CREATE TABLE statements. If
		/// <paramref name="tables"/> is <see langword="null"/>, an empty string
		/// is returned.</param>
		/// <returns>A string containing the SQL CREATE TABLE statements for
		/// each table in the collection, separated by line breaks.
		/// Returns an empty string if <paramref name="tables"/> is
		/// <see langword="null"/> or the collection is empty.</returns>
		public virtual string GetTablesCreateStatements(
			Collection<Table> tables)
		{
			string sqlStatements = string.Empty;

			if (tables != null)
			{
				StringBuilder schemaBuilder = new ();

				for (int index = 0; index < tables.Count; index++)
				{
					Table table = tables[index];

					bool isLast = false;

					if (index == tables.Count - 1)
					{
						isLast = true;
					}

					string statement = GetTableCreateStatement(table, isLast);
					schemaBuilder.AppendLine(statement);
				}

				sqlStatements = schemaBuilder.ToString();
			}

			return sqlStatements;
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
#if NET6_0_OR_GREATER
			ArgumentNullException.ThrowIfNull(column);
#else
			if (column == null)
			{
				throw new ArgumentNullException(nameof(column));
			}
#endif

			string columnType = column.ColumnType switch
			{
				ColumnType.AutoNumber => " INTEGER",
				ColumnType.Currency => " CURRENCY",
				ColumnType.DateTime => " DATETIME",
				ColumnType.Memo => " MEMO",
				ColumnType.Number => " INTEGER",
				ColumnType.Ole => " OLEOBJECT",
				ColumnType.String => string.Format(
					CultureInfo.InvariantCulture,
					" VARCHAR({0})",
					column.Length),
				ColumnType.Text => " TEXT",
				ColumnType.YesNo => " OLEOBJECT",
				_ => string.Empty,
			};

			return columnType;
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
#if NET6_0_OR_GREATER
			ArgumentNullException.ThrowIfNull(table);
#else
			if (table == null)
			{
				throw new ArgumentNullException(nameof(table));
			}
#endif

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
		/// Generates the SQL definition string for the specified column,
		/// including its name, type, constraints, and default value.
		/// </summary>
		/// <remarks>The generated SQL includes the column name, type, and
		/// applicable constraints such as UNIQUE, NOT NULL, IDENTITY, and
		/// DEFAULT. The output is intended for use in table creation scripts
		/// and ends with a comma and newline.</remarks>
		/// <param name="column">The column for which to generate the SQL
		/// definition. Cannot be null.</param>
		/// <param name="isLast">Indicates whether this column is the last
		/// in the list. If <see langword="false"/>, a comma is appended to the
		/// SQL statement.</param>
		/// <returns>A string containing the SQL definition for the column,
		/// formatted for inclusion in a CREATE TABLE statement.</returns>
		protected virtual string GetColumnSql(Column column, bool isLast)
		{
#if NET6_0_OR_GREATER
			ArgumentNullException.ThrowIfNull(column);
#else
			if (column == null)
			{
				throw new ArgumentNullException(nameof(column));
			}
#endif

			string sql = "\t\"" + column.Name + "\"";

			string columnType = GetColumnTypeText(column);
			sql += columnType;

			if (column.Unique)
			{
				sql += " UNIQUE";
			}

			if (column.ColumnType == ColumnType.AutoNumber)
			{
				sql += " IDENTITY";
			}

			if (column.Primary == true)
			{
				sql += " PRIMARY KEY AUTOINCREMENT";
			}
			else if (column.Nullable == false)
			{
				sql += " NOT NULL";
			}

			if (!string.IsNullOrWhiteSpace(column.DefaultValue))
			{
				sql += " DEFAULT " + column.DefaultValue;
			}

			if (isLast == false)
			{
				sql += ",";
			}

			sql += Environment.NewLine;

			return sql;
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
		protected virtual string GetCreateColumnSql(Column column)
		{
#if NET6_0_OR_GREATER
			ArgumentNullException.ThrowIfNull(column);
#else
			if (column == null)
			{
				throw new ArgumentNullException(nameof(column));
			}
#endif

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
#if NET6_0_OR_GREATER
			ArgumentNullException.ThrowIfNull(foreignKey);
#else
			if (foreignKey == null)
			{
				throw new ArgumentNullException(nameof(foreignKey));
			}
#endif

			const string constraint = "CONSTRAINT";
			const string key = "FOREIGN KEY";
			const string references = "REFERENCES";

			const string statement =
				"\t{0} \"{1}\" {2}(\"{3}\") {4} \"{5}\"(\"{6}\")";

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

			if (foreignKey.OnDeleteAction == ConstraintAction.Cascade)
			{
				sql += " ON DELETE CASCADE";
			}
			else if (foreignKey.OnDeleteAction == ConstraintAction.SetNull)
			{
				sql += " ON DELETE SET NULL";
			}

			if (foreignKey.OnUpdateAction == ConstraintAction.Cascade)
			{
				sql += " ON UPDATE CASCADE";
			}
			else if (foreignKey.OnUpdateAction == ConstraintAction.SetNull)
			{
				sql += " ON UPDATE SET NULL";
			}

			if (isLast == false)
			{
				sql += ",";
				sql += Environment.NewLine;
			}

			return sql;
		}
	}
}
