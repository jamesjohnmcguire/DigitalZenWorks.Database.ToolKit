﻿/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseUtilities.cs" company="James John McGuire">
// Copyright © 2006 - 2024 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using DigitalZenWorks.Common.Utilities;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security;

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// Class for common database uses.
	/// </summary>
	public static class DatabaseUtilities
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ResourceManager StringTable = new
			ResourceManager(
			"DigitalZenWorks.Database.ToolKit.Resources",
			Assembly.GetExecutingAssembly());

		/////////////////////////////////////////////////////////////////////
		/// Method <c>CreateAccessDatabaseFile.</c>
		/// <summary>
		/// Creates an empty MDB (MS Jet / Access database) file.
		/// </summary>
		/// <param name="filePath">The file path of the database.</param>
		/// <returns>A values indicating success or not.</returns>
		/////////////////////////////////////////////////////////////////////
		public static bool CreateAccessDatabaseFile(string filePath)
		{
			return FileUtils.CreateFileFromEmbeddedResource(
				"DigitalZenWorks.Database.ToolKit.template.accdb",
				filePath);
		}

		/// <summary>
		/// Export the data table to csv file.
		/// </summary>
		/// <param name="table">The table to export.</param>
		/// <param name="file">The file to export to.</param>
		/// <returns>Indicates whether the action was successful.</returns>
		public static bool ExportDataTableToCsv(
			DataTable table, TextWriter file)
		{
			bool returnCode = false;

			Log.Info(CultureInfo.InvariantCulture, m => m(
				GeneralUtilities.CallingMethod() + ": " +
				StringTable.GetString("BEGIN", CultureInfo.InvariantCulture)));

			if ((null != table) && (null != file))
			{
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

					file.Write(file.NewLine);
				}

				returnCode = true;
			}

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>ExportToCsv.</c>
		/// <summary>
		/// Export all tables to similarly named csv files.
		/// </summary>
		/// <param name="databaseFile">The database file to use.</param>
		/// <param name="csvPath">The csv file to export to.</param>
		/// <returns>A values indicating success or not.</returns>
		/////////////////////////////////////////////////////////////////////
		public static bool ExportToCsv(
			string databaseFile, string csvPath)
		{
			bool returnCode = false;

			DatabaseType databaseType;
			string connectionString;
			string extension = Path.GetExtension(databaseFile);

			if (extension.Equals(".mdb", StringComparison.OrdinalIgnoreCase) ||
				extension.Equals(".accdb", StringComparison.OrdinalIgnoreCase))
			{
				string provider = "Microsoft.ACE.OLEDB.12.0";
				connectionString = string.Format(
					CultureInfo.InvariantCulture,
					"provider={0}; Data Source={1}",
					provider,
					databaseFile);

				databaseType = DatabaseType.OleDb;
			}
			else if (extension.Equals(
					".db", StringComparison.OrdinalIgnoreCase) ||
				extension.Equals(
					".sqlite", StringComparison.OrdinalIgnoreCase))
			{
				connectionString = string.Format(
					CultureInfo.InvariantCulture,
					"Data Source = {0}; Version = 3;",
					databaseFile);

				databaseType = DatabaseType.SQLite;
			}
			else
			{
				throw new NotImplementedException();
			}

			// Open the database
			using (DataStorage database =
				new DataStorage(databaseType, connectionString))
			{
				// Get all the table names
				DataTable tableNames = database.SchemaTable;

				if (null != tableNames)
				{
					// for each table, select all the data
					foreach (DataRow table in tableNames.Rows)
					{
						try
						{
							var objectName = table["TABLE_NAME"];
							string tableName = objectName.ToString();
							string csvFile = csvPath + tableName + ".csv";

#pragma warning disable CA2000

							// Create the CSV file.
							using StreamWriter file = new (csvFile, false);

							// export the table
							string sqlQuery = "SELECT * FROM " + tableName;
							DataTable tableData =
								database.GetDataTable(sqlQuery);

							ExportDataTableToCsv(tableData, file);
#pragma warning restore CA2000
						}
						catch (Exception exception)
						{
							Log.Error(exception.ToString());

							throw;
						}
					}
				}

				database.Shutdown();
			}

			return returnCode;
		}

		/// <summary>
		/// Makes a privileged connection string.
		/// </summary>
		/// <param name="databaseFile">The database file to use.</param>
		/// <returns>The completed connection string.</returns>
		public static string MakePrivilegedConnectString(string databaseFile)
		{
			string provider = "Microsoft.ACE.OLEDB.12.0";

			string connectionString = "Provider=" + provider +
				@";Password="""";User ID=Admin;" + "Data Source=" +
				databaseFile + @";Mode=Share Deny None;" +
				@"Extended Properties="""";" +
				@"Jet OLEDB:System database="""";" +
				@"Jet OLEDB:Registry Path="""";" +
				@"Jet OLEDB:Database Password="""";Jet OLEDB:Engine Type=5;" +
				@"Jet OLEDB:Database Locking Mode=1;" +
				@"Jet OLEDB:Global Partial Bulk Ops=2;" +
				@"Jet OLEDB:Global Bulk Transactions=1;" +
				@"Jet OLEDB:New Database Password="""";" +
				@"Jet OLEDB:Create System Database=False;" +
				@"Jet OLEDB:Encrypt Database=False;" +
				@"Jet OLEDB:Don't Copy Locale on Compact=False;" +
				@"Jet OLEDB:Compact Without Replica Repair=False;" +
				@"Jet OLEDB:SFP=False";

			return connectionString;
		}
	}
}