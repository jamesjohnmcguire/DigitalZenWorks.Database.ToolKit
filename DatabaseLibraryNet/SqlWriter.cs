/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriter.cs" company="James John McGuire">
// Copyright © 2006 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// SQL writer helper class.
	/// </summary>
	/// <param name="rowColumnValues">The row column values.</param>
	public class SqlWriter(string[] rowColumnValues)
	{
		private readonly string[] rowColumnValues = rowColumnValues;

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
				dataItem = dataItem.Replace(
					"'", "''", StringComparison.Ordinal);
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
	}
}
