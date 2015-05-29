/////////////////////////////////////////////////////////////////////////////
// $Id: DataDefinition.cs 38 2015-05-27 14:24:29Z JamesMc $
//
// Copyright (c) 2006-2015 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using DigitalZenWorks.Common.Utils;
using System;
using System.IO;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// Class <c>DataDefinition</c>
	/// <summary>
	/// Class for support on operations on complete data storage containers
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public static class DataDefinition
	{
		/////////////////////////////////////////////////////////////////////
		/// Method <c>ExportSchema</c>
		/// <summary>
		/// Export all tables to similarly named csv files
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public static bool ExportSchema(string databaseFile, string schemaFile)
		{
			//string PrivledgedConnectString =
			//	MakePrivledgedConnectString(databaseFile);

			//System.Collections.Hashtable tables =
			//	new System.Collections.Hashtable();

			//System.Collections.ArrayList relationships =
			//	new System.Collections.ArrayList();

			//DataTable tableNames = GetTableNames();

			//foreach (DataRow row in tableNames.Rows)
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
			//                col.ColumnType = (int)ColumnType.Integer;
			//                break;
			//            case 130:  // String
			//                if (Int32.Parse(r["COLUMN_FLAGS"].ToString()) > 127)
			//                {
			//                    col.ColumnType = (int)ColumnType.Memo;
			//                }
			//                else
			//                {
			//                    col.ColumnType = (int)ColumnType.Str;
			//                }
			//                break;
			//            case 7:  // Date
			//                col.ColumnType = (int)ColumnType.DateTime;
			//                break;
			//            case 6:  // Currency
			//                col.ColumnType = (int)ColumnType.Currency;
			//                break;
			//            case 11:  // Yes/No
			//                col.ColumnType = (int)ColumnType.YesNo;
			//                break;
			//            case 128:  // OLE
			//                col.ColumnType = (int)ColumnType.OLE;
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

		/// <summary>
		/// GetColumnInfo - returns details of a column statement
		/// </summary>
		/// <param name="dataDefinition"></param>
		/// <returns></returns>
		public static string GetColumnInfo(string dataDefinition)
		{
			int SplitIndex = dataDefinition.IndexOf("(") + 1;
			string ColumnsInfo = dataDefinition.Substring(SplitIndex);
			return ColumnsInfo;
		}

		/// <summary>
		/// Returns the column type
		/// </summary>
		/// <param name="Column"></param>
		/// <returns></returns>
		public static ColumnType GetColumnType(
			string Column)
		{
			ColumnType ColumnType = ColumnType.String;

			if (Column.ToLower().Contains("autonumber"))
			{
				ColumnType = ColumnType.AutoNumber;
			}
			if ((Column.ToLower().Contains("identity")) ||
				(Column.ToLower().Contains("autoincrement")))
			{
				ColumnType = ColumnType.Identity;
			}
			else if (Column.ToLower().Contains("bigint"))
			{
				ColumnType = ColumnType.BigInt;
			}
			else if (Column.ToLower().Contains("longvarbinary"))
			{
				ColumnType = ColumnType.LongVarBinary;
			}
			else if (Column.ToLower().Contains("longvarchar"))
			{
				ColumnType = ColumnType.LongVarChar;
			}
			else if (Column.ToLower().Contains("varbinary"))
			{
				ColumnType = ColumnType.VarBinary;
			}
			else if (Column.ToLower().Contains("varchar"))
			{
				ColumnType = ColumnType.VarChar;
			}
			else if (Column.ToLower().Contains("binary"))
			{
				ColumnType = ColumnType.Binary;
			}
			else if (Column.ToLower().Contains("bit"))
			{
				ColumnType = ColumnType.Bit;
			}
			else if (Column.ToLower().Contains("longblob"))
			{
				ColumnType = ColumnType.LongBlob;
			}
			else if (Column.ToLower().Contains("mediumblob"))
			{
				ColumnType = ColumnType.MediumBlob;
			}
			else if (Column.ToLower().Contains("blob"))
			{
				ColumnType = ColumnType.Blob;
			}
			else if (Column.ToLower().Contains("boolean"))
			{
				ColumnType = ColumnType.Boolean;
			}
			else if (Column.ToLower().Contains("byte"))
			{
				ColumnType = ColumnType.Byte;
			}
			else if (Column.ToLower().Contains("nvarchar"))
			{
				ColumnType = ColumnType.NVarChar;
			}
			else if (Column.ToLower().Contains("char"))
			{
				ColumnType = ColumnType.Char;
			}
			else if (Column.ToLower().Contains("currency"))
			{
				ColumnType = ColumnType.Currency;
			}
			else if (Column.ToLower().Contains("cursor"))
			{
				ColumnType = ColumnType.Cursor;
			}
			else if (Column.ToLower().Contains("smalldatetime"))
			{
				ColumnType = ColumnType.SmallDateTime;
			}
			else if (Column.ToLower().Contains("smallint"))
			{
				ColumnType = ColumnType.SmallInt;
			}
			else if (Column.ToLower().Contains("smallmoney"))
			{
				ColumnType = ColumnType.SmallMoney;
			}
			else if (Column.ToLower().Contains("datetime2"))
			{
				ColumnType = ColumnType.DateTime2;
			}
			else if (Column.ToLower().Contains("datetimeoffset"))
			{
				ColumnType = ColumnType.DateTimeOffset;
			}
			else if (Column.ToLower().Contains("datetime"))
			{
				ColumnType = ColumnType.DateTime;
			}
			else if (Column.ToLower().Contains("date"))
			{
				ColumnType = ColumnType.Date;
			}
			else if (Column.ToLower().Contains("decimal"))
			{
				ColumnType = ColumnType.Decimal;
			}
			else if (Column.ToLower().Contains("double"))
			{
				ColumnType = ColumnType.Double;
			}
			else if (Column.ToLower().Contains("hyperlink"))
			{
				ColumnType = ColumnType.Hyperlink;
			}
			else if (Column.ToLower().Contains("enum"))
			{
				ColumnType = ColumnType.Enum;
			}
			else if (Column.ToLower().Contains("float"))
			{
				ColumnType = ColumnType.Float;
			}
			else if (Column.ToLower().Contains("image"))
			{
				ColumnType = ColumnType.Image;
			}
			else if (Column.ToLower().Contains("integer"))
			{
				ColumnType = ColumnType.Integer;
			}
			else if (Column.ToLower().Contains("mediumint"))
			{
				ColumnType = ColumnType.MediumInt;
			}
			else if (Column.ToLower().Contains("tinyint"))
			{
				ColumnType = ColumnType.TinyInt;
			}
			else if (Column.ToLower().Contains("int"))
			{
				ColumnType = ColumnType.Int;
			}
			else if (Column.ToLower().Contains("javaobject"))
			{
				ColumnType = ColumnType.JavaObject;
			}
			else if (Column.ToLower().Contains("longtext"))
			{
				ColumnType = ColumnType.LongText;
			}
			else if (Column.ToLower().Contains("long"))
			{
				ColumnType = ColumnType.Long;
			}
			else if (Column.ToLower().Contains("lookupwizard"))
			{
				ColumnType = ColumnType.LookupWizard;
			}
			else if (Column.ToLower().Contains("mediumtext"))
			{
				ColumnType = ColumnType.MediumText;
			}
			else if (Column.ToLower().Contains("memo"))
			{
				ColumnType = ColumnType.Memo;
			}
			else if (Column.ToLower().Contains("money"))
			{
				ColumnType = ColumnType.Money;
			}
			else if (Column.ToLower().Contains("nchar"))
			{
				ColumnType = ColumnType.NChar;
			}
			else if (Column.ToLower().Contains("ntext"))
			{
				ColumnType = ColumnType.NText;
			}
			else if (Column.ToLower().Contains("number"))
			{
				ColumnType = ColumnType.Number;
			}
			else if (Column.ToLower().Contains("numeric"))
			{
				ColumnType = ColumnType.Numeric;
			}
			else if (Column.ToLower().Contains("oleobject"))
			{
				ColumnType = ColumnType.OleObject;
			}
			else if (Column.ToLower().Contains("ole"))
			{
				ColumnType = ColumnType.Ole;
			}
			else if (Column.ToLower().Contains("real"))
			{
				ColumnType = ColumnType.Real;
			}
			else if (Column.ToLower().Contains("set"))
			{
				ColumnType = ColumnType.Set;
			}
			else if (Column.ToLower().Contains("single"))
			{
				ColumnType = ColumnType.Single;
			}
			else if (Column.ToLower().Contains("sqlvariant"))
			{
				ColumnType = ColumnType.SqlVariant;
			}
			else if (Column.ToLower().Contains("string"))
			{
				ColumnType = ColumnType.String;
			}
			else if (Column.ToLower().Contains("table"))
			{
				ColumnType = ColumnType.Table;
			}
			else if (Column.ToLower().Contains("tinytext"))
			{
				ColumnType = ColumnType.TinyText;
			}
			else if (Column.ToLower().Contains("text"))
			{
				ColumnType = ColumnType.Text;
			}
			else if (Column.ToLower().Contains("timestamp"))
			{
				ColumnType = ColumnType.Timestamp;
			}
			else if (Column.ToLower().Contains("time"))
			{
				ColumnType = ColumnType.Time;
			}
			else if (Column.ToLower().Contains("uniqueidentifier"))
			{
				ColumnType = ColumnType.UniqueIdentifier;
			}
			else if (Column.ToLower().Contains("xml"))
			{
				ColumnType = ColumnType.Xml;
			}
			else if (Column.ToLower().Contains("year"))
			{
				ColumnType = ColumnType.Year;
			}
			else if (Column.ToLower().Contains("yesno"))
			{
				ColumnType = ColumnType.Boolean;
			}
			else
			{
				ColumnType = ColumnType.Other;
			}

			return ColumnType;
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
		/// GetTableName - returns the name of the table
		/// </summary>
		/// <param name="dataDefinition"></param>
		/// <returns></returns>
		public static string GetTableName(string dataDefinition)
		{
			string[] TableParts = dataDefinition.Split(new char[] { '(' });

			string[] TableNameParts = TableParts[0].Split(new char[] { '[', ']' });

			string TableName = TableNameParts[1];

			return TableName;
		}

		/// <summary>
		/// Creates a mdb file with the given schema.
		/// </summary>
		/// <param name="schemaFile"></param>
		/// <param name="databaseFile"></param>
		public static void ImportSchema(string schemaFile, string databaseFile)
		{
			try
			{
				if (File.Exists(schemaFile))
				{
					string provider = "Microsoft.Jet.OLEDB.4.0";
					if (Environment.Is64BitOperatingSystem)
					{
						provider = "Microsoft.ACE.OLEDB.12.0";
					}
					string fileContents = FileUtils.GetFileContents(schemaFile);

					string[] StringSeparators = new string[] { "\r\n\r\n" };
					string[] Queries = fileContents.Split(StringSeparators,
						32000, StringSplitOptions.RemoveEmptyEntries);

					CoreDatabase Database = new CoreDatabase(provider,
						databaseFile);

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

		/// <summary>
		/// Checks to see if the given field is a time related field
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		public static bool IsTimeField(string field)
		{
			bool returnCode = false;

			if ((field.Contains("Time")) || (field.Contains("time")))
			{
				returnCode = true;
			}

			return returnCode;
		}

		/// <summary>
		/// Makes a privileged connection string
		/// </summary>
		/// <param name="databaseFile"></param>
		/// <returns></returns>
		public static string MakePrivledgedConnectString(string databaseFile)
		{
			string provider = "Microsoft.Jet.OLEDB.4.0";
			if (Environment.Is64BitOperatingSystem)
			{
				provider = "Microsoft.ACE.OLEDB.12.0";
			}

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
	} // End class
} // End Namespace