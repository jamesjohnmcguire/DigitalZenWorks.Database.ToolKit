/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseUtilities.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Data;
	using System.IO;
	using System.Security;
	using DigitalZenWorks.Common.Utilities;
	using global::Common.Logging;

	/// <summary>
	/// Class for common database uses.
	/// </summary>
	public static class DatabaseUtilities
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Export database to similarly named csv file.
		/// </summary>
		/// <param name="database">The database file to use.</param>
		/// <param name="csvPath">The csv file to export to.</param>
		/// <returns>A values indicating success or not.</returns>
		public static bool ExportDatabaseToCsv(
			DataStorage database, string csvPath)
		{
			bool returnCode = false;

			ArgumentNullException.ThrowIfNull(database);

			// Get all the table names
			DataTable tableNames = database.SchemaTable;

			if (tableNames != null)
			{
				// for each table, select all the data
				foreach (DataRow table in tableNames.Rows)
				{
					ExportDataRowToCsv(database, table, csvPath);
				}
			}

			returnCode = true;

			return returnCode;
		}

		/// <summary>
		/// Export the data table to csv file.
		/// </summary>
		/// <param name="table">The table to export.</param>
		/// <param name="csvfile">The file to export to.</param>
		/// <returns>Indicates whether the action was successful.</returns>
		public static bool ExportDataTableToCsv(
			DataTable table, string csvfile)
		{
			bool returnCode = false;

			string message =
				GeneralUtilities.CallingMethod() + ": " + Strings.Begin;
			Log.Info(message);

			if (table != null)
			{
				using StreamWriter file = new(csvfile, false);

				// First write the headers.
				int columnCount = table.Columns.Count;

				for (int index = 0; index < columnCount; index++)
				{
					file.Write("\"");
					file.Write(table.Columns[index]);
					if (index < columnCount - 1)
					{
						file.Write("\", ");
					}
				}

				file.Write(file.NewLine);

				// Now write all the rows.
				foreach (DataRow row in table.Rows)
				{
					for (int index = 0; index < columnCount; index++)
					{
						WriteColumnToCsvFile(row, file, columnCount, index);
					}

					file.Write(file.NewLine);
				}

				returnCode = true;
			}

			return returnCode;
		}

		/// Method <c>ExportToCsv.</c>
		/// <summary>
		/// Export all tables to similarly named csv files.
		/// </summary>
		/// <param name="databaseFile">The database file to use.</param>
		/// <param name="csvPath">The csv file to export to.</param>
		/// <returns>A values indicating success or not.</returns>
		public static bool ExportToCsv(
			string databaseFile, string csvPath)
		{
			bool returnCode = false;

			DatabaseType databaseType =
				DataDefinition.GetDatabaseType(databaseFile);
			string connectionString =
				DataStorage.GetConnectionString(databaseType, databaseFile);

			using DataStorage database = new(databaseType, connectionString);

			returnCode = ExportDatabaseToCsv(database, csvPath);

			database.Shutdown();

			return returnCode;
		}

		private static bool ExportDataRowToCsv(
			DataStorage database, DataRow row, string csvPath)
		{
			bool returnCode = false;

			try
			{
				object objectName = row["TABLE_NAME"];
				string tableName = objectName.ToString();

				// export the table
				string sqlQuery = "SELECT * FROM " + tableName;
				DataTable tableData = database.GetDataTable(sqlQuery);

				string csvFile = csvPath + tableName + ".csv";

				ExportDataTableToCsv(tableData, csvFile);
			}
			catch (Exception exception) when
				(exception is ArgumentException ||
				exception is ArgumentNullException ||
				exception is DirectoryNotFoundException ||
				exception is IOException ||
				exception is PathTooLongException ||
				exception is SecurityException ||
				exception is UnauthorizedAccessException)
			{
				Log.Error(exception.ToString());
			}
			catch (Exception exception)
			{
				Log.Error(exception.ToString());

				throw;
			}

			returnCode = true;

			return returnCode;
		}

		private static void WriteColumnToCsvFile(
			DataRow row, TextWriter file, int columnCount, int index)
		{
			file.Write("\"");
			if (!row.IsNull(index))
			{
				file.Write(row[index].ToString());
			}

			if (index < columnCount - 1)
			{
				file.Write("\", ");
			}
		}
	}
}
