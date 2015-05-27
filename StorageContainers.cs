/////////////////////////////////////////////////////////////////////////////
// $Id$
//
// Copyright (c) 2006-2015 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;

using Zenware.Common.UtilsNet;

namespace Zenware.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>StorageContainers</c>
	/// <summary>
	/// Class for support on operations on complete data storage containers
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class StorageContainers
	{
		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents a provider type for a connection string
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private string provider = "Microsoft.Jet.OLEDB.4.0";

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Default Constructor
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public StorageContainers()
		{
			if (Environment.Is64BitOperatingSystem)
			{
				provider = "Microsoft.ACE.OLEDB.12.0";
			}
		}

		/// <summary>
		/// Export the data table to csv file.
		/// </summary>
		/// <param name="ExportTable"></param>
		/// <param name="ExportFile"></param>
		/// <returns></returns>
		public static bool ExportDataTableToCsv(
			DataTable ExportTable,
			StreamWriter ExportFile)
		{
			bool ReturnCode = false;

			// First write the headers.
			int ColumnCount = ExportTable.Columns.Count;

			for (int Index = 0; Index < ColumnCount; Index++)
			{
				ExportFile.Write("\"");
				ExportFile.Write(ExportTable.Columns[Index]);
				if (Index < ColumnCount - 1)
				{
					ExportFile.Write("\", ");
				}
			}

			ExportFile.Write(ExportFile.NewLine);

			// Now write all the rows.
			foreach (DataRow Row in ExportTable.Rows)
			{
				for (int Index = 0; Index < ColumnCount; Index++)
				{
					ExportFile.Write("\"");
					if (!Convert.IsDBNull(Row[Index]))
					{
						ExportFile.Write(Row[Index].ToString());
					}
					if (Index < ColumnCount - 1)
					{
						ExportFile.Write("\", ");
					}
				}

				ExportFile.Write(ExportFile.NewLine);
			}

			ReturnCode = true;

			return ReturnCode;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>ExportToCsv</c>
		/// <summary>
		/// Export all tables to similarly named csv files
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public static bool ExportToCsv(
			string DatebaseFile,
			string CsvPath)
		{
			bool ReturnCode = false;

			// Open the database
			string provider = "Microsoft.Jet.OLEDB.4.0";
			if (Environment.Is64BitOperatingSystem)
			{
				provider = "Microsoft.ACE.OLEDB.12.0";
			}
			CoreDatabase Database = new CoreDatabase(provider, DatebaseFile);

			// Get all the table names
			DataTable TableNames = GetTableNames(Database);

			// for each table, select all the data
			foreach (DataRow Table in TableNames.Rows)
			{
				string TableName = Table["TABLE_NAME"].ToString();
				string CsvFile = CsvPath + "\\" + TableName + ".csv";

				// Create the CSV file to which data will be exported.
				StreamWriter ExportFile = new StreamWriter(CsvFile, false);

				// export the table
				string SqlQuery = "SELECT * FROM " + Table["TABLE_NAME"].ToString();
				DataTable TableData = null;
				int RowCount = Database.GetDataTable(SqlQuery, out TableData);

				ExportDataTableToCsv(TableData, ExportFile);

				ExportFile.Close();
			}

			Database.Shutdown();

			return ReturnCode;
		}

		/// <summary>
		/// GetColumnsInfo - returns details of a column statement
		/// </summary>
		/// <param name="DdlStatement"></param>
		/// <returns></returns>
		public static string GetColumnsInfo(string DdlStatement)
		{
			int SplitIndex = DdlStatement.IndexOf("(") + 1;
			string ColumnsInfo = DdlStatement.Substring(SplitIndex);
			return ColumnsInfo;
		}

		/// <summary>
		/// GetTableDefinitions - returns an array of table definitions
		/// </summary>
		/// <param name="TableDefinitionsFile"></param>
		/// <returns></returns>
		public static string[] GetTableDefinitions(
			string TableDefinitionsFile)
		{
			string[] StringSeparators = new string[] { "\r\n\r\n" };
			string[] Queries = TableDefinitionsFile.Split(StringSeparators,
				32000, StringSplitOptions.RemoveEmptyEntries);

			return Queries;
		}

		/// <summary>
		/// GetTableNameFromTableDefinitions - returns the name of the table
		/// </summary>
		/// <param name="DdlStatement"></param>
		/// <returns></returns>
		public static string GetTableNameFromTableDefinitions(
			string DdlStatement)
		{
			string[] TableParts = DdlStatement.Split(new char[] { '(' });

			string[] TableNameParts = TableParts[0].Split(new char[] { '[', ']' });

			string TableName = TableNameParts[1];

			return TableName;
		}

		/// <summary>
		/// Get table names from the internal schema.
		/// </summary>
		/// <param name="Database"></param>
		/// <returns></returns>
		public static DataTable GetTableNames(
			CoreDatabase Database)
		{
			DataTable Tables = Database.GetSchemaTable();

			return Tables;
		}

		/// <summary>
		/// Returns the column type
		/// </summary>
		/// <param name="Column"></param>
		/// <returns></returns>
		public static ColumnTypes GetColumnType(
			string Column)
		{
			ColumnTypes ColumnType = ColumnTypes.String;

			if (Column.ToLower().Contains("autonumber"))
			{
				ColumnType = ColumnTypes.AutoNumber;
			}
			if ((Column.ToLower().Contains("identity")) ||
				(Column.ToLower().Contains("autoincrement")))
			{
				ColumnType = ColumnTypes.Identity;
			}
			else if (Column.ToLower().Contains("bigint"))
			{
				ColumnType = ColumnTypes.BigInt;
			}
			else if (Column.ToLower().Contains("longvarbinary"))
			{
				ColumnType = ColumnTypes.LongVarBinary;
			}
			else if (Column.ToLower().Contains("longvarchar"))
			{
				ColumnType = ColumnTypes.LongVarChar;
			}
			else if (Column.ToLower().Contains("varbinary"))
			{
				ColumnType = ColumnTypes.VarBinary;
			}
			else if (Column.ToLower().Contains("varchar"))
			{
				ColumnType = ColumnTypes.VarChar;
			}
			else if (Column.ToLower().Contains("binary"))
			{
				ColumnType = ColumnTypes.Binary;
			}
			else if (Column.ToLower().Contains("bit"))
			{
				ColumnType = ColumnTypes.Bit;
			}
			else if (Column.ToLower().Contains("longblob"))
			{
				ColumnType = ColumnTypes.LongBlob;
			}
			else if (Column.ToLower().Contains("mediumblob"))
			{
				ColumnType = ColumnTypes.MediumBlob;
			}
			else if (Column.ToLower().Contains("blob"))
			{
				ColumnType = ColumnTypes.Blob;
			}
			else if (Column.ToLower().Contains("boolean"))
			{
				ColumnType = ColumnTypes.Boolean;
			}
			else if (Column.ToLower().Contains("byte"))
			{
				ColumnType = ColumnTypes.Byte;
			}
			else if (Column.ToLower().Contains("nvarchar"))
			{
				ColumnType = ColumnTypes.NVarChar;
			}
			else if (Column.ToLower().Contains("char"))
			{
				ColumnType = ColumnTypes.Char;
			}
			else if (Column.ToLower().Contains("currency"))
			{
				ColumnType = ColumnTypes.Currency;
			}
			else if (Column.ToLower().Contains("cursor"))
			{
				ColumnType = ColumnTypes.Cursor;
			}
			else if (Column.ToLower().Contains("smalldatetime"))
			{
				ColumnType = ColumnTypes.SmallDateTime;
			}
			else if (Column.ToLower().Contains("smallint"))
			{
				ColumnType = ColumnTypes.SmallInt;
			}
			else if (Column.ToLower().Contains("smallmoney"))
			{
				ColumnType = ColumnTypes.SmallMoney;
			}
			else if (Column.ToLower().Contains("datetime2"))
			{
				ColumnType = ColumnTypes.DateTime2;
			}
			else if (Column.ToLower().Contains("datetimeoffset"))
			{
				ColumnType = ColumnTypes.DateTimeOffset;
			}
			else if (Column.ToLower().Contains("datetime"))
			{
				ColumnType = ColumnTypes.DateTime;
			}
			else if (Column.ToLower().Contains("date"))
			{
				ColumnType = ColumnTypes.Date;
			}
			else if (Column.ToLower().Contains("decimal"))
			{
				ColumnType = ColumnTypes.Decimal;
			}
			else if (Column.ToLower().Contains("double"))
			{
				ColumnType = ColumnTypes.Double;
			}
			else if (Column.ToLower().Contains("hyperlink"))
			{
				ColumnType = ColumnTypes.HyperLink;
			}
			else if (Column.ToLower().Contains("enum"))
			{
				ColumnType = ColumnTypes.Enum;
			}
			else if (Column.ToLower().Contains("float"))
			{
				ColumnType = ColumnTypes.Float;
			}
			else if (Column.ToLower().Contains("image"))
			{
				ColumnType = ColumnTypes.Image;
			}
			else if (Column.ToLower().Contains("integer"))
			{
				ColumnType = ColumnTypes.Integer;
			}
			else if (Column.ToLower().Contains("mediumint"))
			{
				ColumnType = ColumnTypes.MediumInt;
			}
			else if (Column.ToLower().Contains("tinyint"))
			{
				ColumnType = ColumnTypes.TinyInt;
			}
			else if (Column.ToLower().Contains("int"))
			{
				ColumnType = ColumnTypes.Int;
			}
			else if (Column.ToLower().Contains("javaobject"))
			{
				ColumnType = ColumnTypes.JavaObject;
			}
			else if (Column.ToLower().Contains("longtext"))
			{
				ColumnType = ColumnTypes.LongText;
			}
			else if (Column.ToLower().Contains("long"))
			{
				ColumnType = ColumnTypes.Long;
			}
			else if (Column.ToLower().Contains("lookupwizard"))
			{
				ColumnType = ColumnTypes.LookupWizard;
			}
			else if (Column.ToLower().Contains("mediumtext"))
			{
				ColumnType = ColumnTypes.MediumText;
			}
			else if (Column.ToLower().Contains("memo"))
			{
				ColumnType = ColumnTypes.Memo;
			}
			else if (Column.ToLower().Contains("money"))
			{
				ColumnType = ColumnTypes.Money;
			}
			else if (Column.ToLower().Contains("nchar"))
			{
				ColumnType = ColumnTypes.NChar;
			}
			else if (Column.ToLower().Contains("ntext"))
			{
				ColumnType = ColumnTypes.NText;
			}
			else if (Column.ToLower().Contains("number"))
			{
				ColumnType = ColumnTypes.Number;
			}
			else if (Column.ToLower().Contains("numeric"))
			{
				ColumnType = ColumnTypes.Numeric;
			}
			else if (Column.ToLower().Contains("oleobject"))
			{
				ColumnType = ColumnTypes.OleObject;
			}
			else if (Column.ToLower().Contains("ole"))
			{
				ColumnType = ColumnTypes.Ole;
			}
			else if (Column.ToLower().Contains("real"))
			{
				ColumnType = ColumnTypes.Real;
			}
			else if (Column.ToLower().Contains("set"))
			{
				ColumnType = ColumnTypes.Set;
			}
			else if (Column.ToLower().Contains("single"))
			{
				ColumnType = ColumnTypes.Single;
			}
			else if (Column.ToLower().Contains("sqlvariant"))
			{
				ColumnType = ColumnTypes.SqlVariant;
			}
			else if (Column.ToLower().Contains("string"))
			{
				ColumnType = ColumnTypes.String;
			}
			else if (Column.ToLower().Contains("table"))
			{
				ColumnType = ColumnTypes.Table;
			}
			else if (Column.ToLower().Contains("tinytext"))
			{
				ColumnType = ColumnTypes.TinyText;
			}
			else if (Column.ToLower().Contains("text"))
			{
				ColumnType = ColumnTypes.Text;
			}
			else if (Column.ToLower().Contains("timestamp"))
			{
				ColumnType = ColumnTypes.TimeStamp;
			}
			else if (Column.ToLower().Contains("time"))
			{
				ColumnType = ColumnTypes.Time;
			}
			else if (Column.ToLower().Contains("uniqueidentifier"))
			{
				ColumnType = ColumnTypes.UniqueIdentifier;
			}
			else if (Column.ToLower().Contains("xml"))
			{
				ColumnType = ColumnTypes.Xml;
			}
			else if (Column.ToLower().Contains("year"))
			{
				ColumnType = ColumnTypes.Year;
			}
			else if (Column.ToLower().Contains("yesno"))
			{
				ColumnType = ColumnTypes.Boolean;
			}
			else
			{
				ColumnType = ColumnTypes.Other;
			}

			return ColumnType;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>CreateMdbFile</c>
		/// <summary>
		/// Creates an empty MDB (MS Jet / Access database) file.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public static void CreateMdbFile(
			string NewFilePath)
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
				NewFileStream = new FileStream(NewFilePath, FileMode.Create);
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
		/// Creates a mdb file with the given schema.
		/// </summary>
		/// <param name="SchemaFile"></param>
		/// <param name="MdbFile"></param>
		public static void ImportSchema(
			string SchemaFile,
			string MdbFile)
		{
			try
			{
				if (File.Exists(SchemaFile))
				{
					string provider = "Microsoft.Jet.OLEDB.4.0";
					if (Environment.Is64BitOperatingSystem)
					{
						provider = "Microsoft.ACE.OLEDB.12.0";
					}
					string fileContents = FileUtils.GetFileContents(SchemaFile);

					string[] StringSeparators = new string[] { "\r\n\r\n" };
					string[] Queries = fileContents.Split(StringSeparators,
															32000,
															StringSplitOptions.RemoveEmptyEntries);

					CoreDatabase Database = new CoreDatabase(provider,
						MdbFile);

					foreach (string SqlQuery in Queries)
					{
						Database.ExecuteNonQuery(SqlQuery);
					}

					Database.Shutdown();
				}
			}
			catch (Exception Ex)
			{
				Console.WriteLine("Exception: " + Ex.ToString());
			}
		}

		private bool IsTimeField(
			string Field)
		{
			bool ReturnCode = false;

			if ((Field.Contains("Time")) || (Field.Contains("time")))
			{
				ReturnCode = true;
			}

			return ReturnCode;
		}

		private string MakePrivledgedConnectString(
			string MdbFile)
		{
			return @"Provider=" + provider + @";Password="""";User ID=Admin;"
					+ "Data Source=" + MdbFile + @";Mode=Share Deny None;Extended Properties="""";Jet OLEDB:System database="""";"
					+ @"Jet OLEDB:Registry Path="""";Jet OLEDB:Database Password="""";Jet OLEDB:Engine Type=5;"
					+ @"Jet OLEDB:Database Locking Mode=1;Jet OLEDB:Global Partial Bulk Ops=2;Jet OLEDB:Global Bulk Transactions=1;"
					+ @"Jet OLEDB:New Database Password="""";Jet OLEDB:Create System Database=False;Jet OLEDB:Encrypt Database=False;"
					+ @"Jet OLEDB:Don't Copy Locale on Compact=False;Jet OLEDB:Compact Without Replica Repair=False;Jet OLEDB:SFP=False";
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>ExportSchema</c>
		/// <summary>
		/// Export all tables to similarly named csv files
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public bool ExportSchema(
			string MdbFile,
			string SqlFile)
		{
			string PrivledgedConnectString = MakePrivledgedConnectString(MdbFile);

			System.Collections.Hashtable tables = new System.Collections.Hashtable();

			System.Collections.ArrayList relationships = new System.Collections.ArrayList();

			//DataTable t = GetTableNames();

			//foreach (DataRow row in t.Rows)
			//{
			//    string table_name = row["TABLE_NAME"].ToString();

			//    Table table = new Table(table_name);

			//    Console.WriteLine("Getting Columns for " + table_name);
			//    DataTable cols = GetTableColumns(table_name);

			//    foreach (DataRow r in cols.Rows)
			//    {
			//        Column col = new Column();
			//        col.Name = r["COLUMN_NAME"].ToString();

			//        switch ((int)r["DATA_TYPE"])
			//        {
			//            case 3:	// Integer
			//                col.ColumnType = (int)ColumnTypes.Integer;
			//                break;
			//            case 130:  // String
			//                if (Int32.Parse(r["COLUMN_FLAGS"].ToString()) > 127)
			//                {
			//                    col.ColumnType = (int)ColumnTypes.Memo;
			//                }
			//                else
			//                {
			//                    col.ColumnType = (int)ColumnTypes.Str;
			//                }
			//                break;
			//            case 7:  // Date
			//                col.ColumnType = (int)ColumnTypes.DateTime;
			//                break;
			//            case 6:  // Currency
			//                col.ColumnType = (int)ColumnTypes.Currency;
			//                break;
			//            case 11:  // Yes/No
			//                col.ColumnType = (int)ColumnTypes.YesNo;
			//                break;
			//            case 128:  // OLE
			//                col.ColumnType = (int)ColumnTypes.OLE;
			//                break;
			//        }

			//        if (!r.IsNull("CHARACTER_MAXIMUM_LENGTH"))
			//            col.Length = Int32.Parse(r["CHARACTER_MAXIMUM_LENGTH"].ToString());

			//        if (r["IS_NULLABLE"].ToString() == "True") col.Nullable = true;

			//        if (r["COLUMN_HASDEFAULT"].ToString() == "True")
			//            col.DefaultValue = r["COLUMN_DEFAULT"].ToString();

			//        col.Position = Int32.Parse(r["ORDINAL_POSITION"].ToString());

			//        table.AddColumn(col);
			//    }
			//}
			//string sqlString = "";

			//// Get Sorted List
			//ArrayList list = OrderTable(tables);

			//foreach (string name in list)
			//{
			//    sqlString += WriteSQL((Table)tables[name]);
			//    sqlString += Environment.NewLine;
			//}

			return true;
		}
	} // End class
} // End Namespace