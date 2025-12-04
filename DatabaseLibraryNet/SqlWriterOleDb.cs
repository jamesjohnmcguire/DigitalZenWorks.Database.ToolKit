/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterOleDb.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;

	/// <summary>
	/// SQL writer helper for OleDb class.
	/// </summary>
	public class SqlWriterOleDb : SqlWriter
	{
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
		public override string GetTableCreateStatement(Table table)
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

			bool isPrimaryKeyAdded = false;

			if (!string.IsNullOrWhiteSpace(table.PrimaryKey))
			{
				sql += string.Format(
					CultureInfo.InvariantCulture,
					"\tCONSTRAINT PrimaryKey PRIMARY KEY ([{0}])",
					table.PrimaryKey);

				isPrimaryKeyAdded = true;
			}

			for (int index = 0; index < table.ForeignKeys.Count; index++)
			{
				ForeignKey foreignKey = table.ForeignKeys[index];

				if (index == 0 && isPrimaryKeyAdded == true)
				{
					sql += "," + Environment.NewLine;
				}

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
		protected override string GetCreateColumnSql(Column column)
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

			const string constraint = "CONSTRAINT";
			const string key = "FOREIGN KEY";
			const string references = "REFERENCES";

			const string statement =
				"\t{0} [{1}] {2} ([{3}]) {4} [{5}] ([{6}])";

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
				sql += " SET NULL";
			}

			if (foreignKey.OnUpdateAction == ConstraintAction.Cascade)
			{
				sql += " ON DELETE CASCADE";
			}
			else if (foreignKey.OnUpdateAction == ConstraintAction.SetNull)
			{
				sql += " SET NULL";
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
