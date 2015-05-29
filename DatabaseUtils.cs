﻿/////////////////////////////////////////////////////////////////////////////
// $Id: DataDefinition.cs 38 2015-05-27 14:24:29Z JamesMc $
//
// Copyright (c) 2006-2015 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/// <summary>
	/// Class for common database uses
	/// </summary>
	public class DatabaseUtils
	{
		/////////////////////////////////////////////////////////////////////
		/// Method <c>CreateMdbFile</c>
		/// <summary>
		/// Creates an empty MDB (MS Jet / Access database) file.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public static void CreateMdbFile(string filePath)
		{
			Stream TemplateObjectStream = null;
			FileStream NewFileStream = null;

			try
			{
				byte[] EmbeddedResource;
				Assembly ThisAssembly = Assembly.GetExecutingAssembly();

				TemplateObjectStream = ThisAssembly.GetManifestResourceStream(
					"DatabaseLibaryNet.template.mdb");
				EmbeddedResource = new Byte[TemplateObjectStream.Length];
				TemplateObjectStream.Read(EmbeddedResource, 0,
					(int)TemplateObjectStream.Length);
				NewFileStream = new FileStream(filePath, FileMode.Create);
				NewFileStream.Write(EmbeddedResource, 0,
					(int)TemplateObjectStream.Length);
				NewFileStream.Close();
			}
			catch (Exception Ex)
			{
				Console.WriteLine("Exception: " + Ex.ToString());
			}
		}

		/// <summary>
		/// Export the data table to csv file.
		/// </summary>
		/// <param name="table"></param>
		/// <param name="file"></param>
		/// <returns></returns>
		public static bool ExportDataTableToCsv(DataTable table,
			StreamWriter file)
		{
			bool returnCode = false;

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
			foreach (DataRow Row in table.Rows)
			{
				for (int Index = 0; Index < ColumnCount; Index++)
				{
					file.Write("\"");
					if (!Convert.IsDBNull(Row[Index]))
					{
						file.Write(Row[Index].ToString());
					}
					if (Index < ColumnCount - 1)
					{
						file.Write("\", ");
					}
				}

				file.Write(file.NewLine);
			}

			returnCode = true;

			return returnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>ExportToCsv</c>
		/// <summary>
		/// Export all tables to similarly named csv files
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public static bool ExportToCsv(string databaseFile,
			string CsvPath)
		{
			bool returnCode = false;

			// Open the database
			string provider = "Microsoft.Jet.OLEDB.4.0";
			if (Environment.Is64BitOperatingSystem)
			{
				provider = "Microsoft.ACE.OLEDB.12.0";
			}
			CoreDatabase Database = new CoreDatabase(provider, databaseFile);

			// Get all the table names
			DataTable TableNames = Database.GetSchemaTable();

			// for each table, select all the data
			foreach (DataRow Table in TableNames.Rows)
			{
				string TableName = Table["TABLE_NAME"].ToString();
				string CsvFile = CsvPath + "\\" + TableName + ".csv";

				// Create the CSV file to which data will be exported.
				StreamWriter file = new StreamWriter(CsvFile, false);

				// export the table
				string SqlQuery = "SELECT * FROM " + Table["TABLE_NAME"].ToString();
				DataTable TableData = null;
				int RowCount = Database.GetDataTable(SqlQuery, out TableData);

				ExportDataTableToCsv(TableData, file);

				file.Close();
			}

			Database.Shutdown();

			return returnCode;
		}

	}
}
