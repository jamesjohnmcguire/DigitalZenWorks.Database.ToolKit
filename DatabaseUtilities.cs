/////////////////////////////////////////////////////////////////////////////
// $Id: DataDefinition.cs 38 2015-05-27 14:24:29Z JamesMc $
//
// Copyright © 2006-2016 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using Common.Logging;
using DigitalZenWorks.Common.Utils;
using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Security;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/// <summary>
	/// Class for common database uses
	/// </summary>
	public static class DatabaseUtilities
	{
		private static readonly ILog log = LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		private static readonly ResourceManager stringTable = new
			ResourceManager("DigitalZenWorks.Common.DatabaseLibrary.Resources",
			Assembly.GetExecutingAssembly());

		/////////////////////////////////////////////////////////////////////
		/// Method <c>CreateAccessDatabaseFile</c>
		/// <summary>
		/// Creates an empty MDB (MS Jet / Access database) file.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public static bool CreateAccessDatabaseFile(string filePath)
		{
			return FileUtils.CreateFileFromEmbeddedResource(
				"DigitalZenWorks.Common.DatabaseLibrary.template.accdb",
				filePath);
		}

		/// <summary>
		/// Export the data table to csv file.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="file"></param>
		/// <returns></returns>
		public static bool ExportDataTableToCsv(DataTable table,
			TextWriter file)
		{
			bool returnCode = false;

			log.Info(CultureInfo.InvariantCulture, m => m(
				General.CallingMethod() + ": " +
				stringTable.GetString("BEGIN")));

			if ((null != table) && (null != file))
			{
				// First write the headers.
				int ColumnCount = table.Columns.Count;

				for (int Index = 0; Index < ColumnCount; Index++)
				{
					file.Write("\"");
					file.Write(table.Columns[Index]);
					if (Index < ColumnCount - 1)
					{
						file.Write("\", ");
					}
				}

				file.Write(file.NewLine);

				// Now write all the rows.
				foreach (DataRow row in table.Rows)
				{
					for (int Index = 0; Index < ColumnCount; Index++)
					{
						file.Write("\"");
						if (!row.IsNull(Index))
						{
							file.Write(row[Index].ToString());
						}
						if (Index < ColumnCount - 1)
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
		/// Method <c>ExportToCsv</c>
		/// <summary>
		/// Export all tables to similarly named csv files
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public static bool ExportToCsv(string databaseFile,
			string csvPath)
		{
			bool returnCode = false;

			// Open the database
			string provider = "Microsoft.ACE.OLEDB.12.0";

			using (DataStorage Database =
				new DataStorage(provider, databaseFile))
			{
				// Get all the table names
				DataTable TableNames = Database.SchemaTable;

				if (null != TableNames)
				{
					// for each table, select all the data
					foreach (DataRow Table in TableNames.Rows)
					{
						string TableName = Table["TABLE_NAME"].ToString();
						string csvFile = csvPath + TableName + ".csv";

						// Create the CSV file to which data will be exported.
						using (StreamWriter file =
							new StreamWriter(csvFile, false))
						{
							// export the table
							string SqlQuery = "SELECT * FROM " +
								Table["TABLE_NAME"].ToString();
							DataTable TableData =
								Database.GetDataTable(SqlQuery);

							ExportDataTableToCsv(TableData, file);
						}
					}
				}

				Database.Shutdown();
			}
			return returnCode;
		}

		/// <summary>
		/// Makes a privileged connection string
		/// </summary>
		/// <param name="databaseFile"></param>
		/// <returns></returns>
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