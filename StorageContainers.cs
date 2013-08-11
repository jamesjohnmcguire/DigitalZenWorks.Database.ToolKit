/////////////////////////////////////////////////////////////////////////////
// StorageContainers Class
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
	/// Class for support on operations on complete data storage containters
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class StorageContainers
	{
		private enum ColumnTypes
		{
			Autonumber,
			Currency,
			DateTime,
			Memo,
			Number,
			Ole,
			String,
			YesNo
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>CreateMdbFile</c>
		/// <summary>
		/// Creates an empty MDB (MS Jet / Access databse) file.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		private void CreateMdbFile(
			string NewFilePath)
		{
			Stream TemplateObjectStream = null;
			FileStream NewFileStream = null;

			try
			{
				byte[] EmbeddedResource;
				Assembly ThisAssembly = Assembly.GetExecutingAssembly();

				TemplateObjectStream = ThisAssembly.GetManifestResourceStream("DatabaseLibaryNet.template.mdb");
				EmbeddedResource = new Byte[TemplateObjectStream.Length];
				TemplateObjectStream.Read(EmbeddedResource, 0, (int)TemplateObjectStream.Length);
				NewFileStream = new FileStream(NewFilePath, FileMode.Create);
				NewFileStream.Write(EmbeddedResource, 0, (int)TemplateObjectStream.Length);
				NewFileStream.Close();
			}
			catch (Exception Ex)
			{
				Console.WriteLine("Exception: " + Ex.ToString());
			}
		}

		private void ImportSchema(
			string SchemaFile,
			string MdbFile)
		{
			try
			{
				if (File.Exists(SchemaFile))
				{
					string FileContents = Utils.GetFileContents(SchemaFile);

					string[] StringSeparators = new string[] { "\r\n\r\n" };
					string[] Queries = FileContents.Split(StringSeparators,
															32000,
															StringSplitOptions.RemoveEmptyEntries);

					CoreDatabase Database = new CoreDatabase("DatabaseLibaryNET",
															"Microsoft.Jet.OLEDB.4.0",
															MdbFile);

					bool ReturnCode = Database.Initialize();

					if (true == ReturnCode)
					{
						foreach (string SqlQuery in Queries)
						{
							Database.ExecuteNonQuery(SqlQuery);
						}

						Database.ShutDown();
					}
				}
			}
			catch (Exception Ex)
			{
				Console.WriteLine("Exception: " + Ex.ToString());
			}
		}

		private string[] GetTableDefinitions(
			string TableDefinitionsFile)
		{
			string[] StringSeparators = new string[] { "\r\n\r\n" };
			string[] Queries = TableDefinitionsFile.Split(StringSeparators,
													32000,
													StringSplitOptions.RemoveEmptyEntries);

			return Queries;
		}

		private string GetTableNameFromTableDefinitions(
			string DdlStatement)
		{
			string[] TableParts = DdlStatement.Split(new char[] { '(' });

			string[] TableNameParts = TableParts[0].Split(new char[] { '[', ']' });

			string TableName = TableNameParts[1];

			return TableName;
		}

		private string GetColumnsInfo(
			string DdlStatement)
		{
			int SplitIndex = DdlStatement.IndexOf("(") + 1;
			string ColumnsInfo = DdlStatement.Substring(SplitIndex);
			return ColumnsInfo;
		}

		private string GetFileHeader()
		{
			string FileHeader = "using System;" +
				"\r\nusing System.Data;" +
				"\r\n\r\nusing Zenware.DatabaseLibrary;" +
				"\r\nusing Zenware.DiagnosticsLibrary;" +
				"\r\n\r\nnamespace Zenware.Contacts.BusinessLogicLayer" +
				"\r\n{" +
				"\r\n\tpublic class ContactsUpdate" +
				"\r\n\t{" +
				"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
				"\r\n\t\t/// <summary>" +
				"\r\n\t\t/// The core database object" +
				"\r\n\t\t/// </summary>" +
				"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
				"\r\n\t\tprivate CoreDatabase m_Database = null;" +
				"\r\n\r\n\t\t/////////////////////////////////////////////////////////////////////" +
				"\r\n\t\t/// Method <c>ContactsUpdate</c>" +
				"\r\n\t\t/// <summary>" +
				"\r\n\t\t/// Default constructor" +
				"\r\n\t\t/// </summary>" +
				"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
				"\r\n\t\tpublic ContactsUpdate(" +
				"\r\n\t\t\tCoreDatabase DatabaseObject)" +
				"\r\n\t\t{" +
				"\r\n\t\t\tm_Database = DatabaseObject;" +
				"\r\n\t\t}";

			return FileHeader;
		}

		private string GetFunctionHeader(
			string TableName)
		{
			string FunctionHeader = "\r\n\r\n\t\t/////////////////////////////////////////////////////////////////////" +
				"\r\n\t\t/// Method <c>" + TableName + "</c>" +
				"\r\n\t\t/// <summary>" +
				"\r\n\t\t/// Updates a " + TableName + " record" +
				"\r\n\t\t/// </summary>" +
				"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
				"\r\n\t\tpublic bool " + TableName + "(";

			return FunctionHeader;
		}

		private string GetTestFileHeader()
		{
			string userDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			string TestFileContents = "using System;" +
										"\r\nusing System.Data;" +
										"\r\nusing NUnit.Framework;" +
										"\r\nusing Zenware.DatabaseLibrary;" +
										"\r\n\r\nnamespace Zenware.Contacts.BusinessLogicLayer.UnitTests" +
										"\r\n{" +
										"\r\n\t/////////////////////////////////////////////////////////////////////////" +
										"\r\n\t/// Class <c>UpdateTests</c>" +
										"\r\n\t/// <summary>" +
										"\r\n\t/// Database UpdateTests Unit Testing Class" +
										"\r\n\t/// </summary>" +
										"\r\n\t/////////////////////////////////////////////////////////////////////////" +
										"\r\n\t[TestFixture]" +
										"\r\n\tpublic class UpdateTests" +
										"\r\n\t{" +
										"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
										"\r\n\t\t/// <summary>" +
										"\r\n\t\t/// The core database object" +
										"\r\n\t\t/// </summary>" +
										"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
										"\r\n\t\tprivate CoreDatabase m_Database = null;" +
										"\r\n\r\n\t\t/////////////////////////////////////////////////////////////////////" +
										"\r\n\t\t/// <summary>" +
										"\r\n\t\t/// " +
										"\r\n\t\t/// </summary>" +
										"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
										"\r\n\t\tprivate ContactsUpdate m_ContactsUpdate = null;" +
										"\r\n" +
										"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
										"\r\n\t\t/// Method <c>SetUp</c>" +
										"\r\n\t\t/// <summary>" +
										"\r\n\t\t/// function that is called just before each test method is called." +
										"\r\n\t\t/// </summary>" +
										"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
										"\r\n\t\t[SetUp]" +
										"\r\n\t\tpublic void SetUp()" +
										"\r\n\t\t{" +
										"\r\n\t\t\tm_Database = new CoreDatabase(" +
										"\r\n\t\t\t\t\"ContactsPlus\"," +
										"\r\n\t\t\t\t\"Microsoft.Jet.OLEDB.4.0\"," +
										"\r\n\t\t\t\t@\"" + userDataFolder + "\\data\\admin\\Contacts\\ContactsX.mdb\");" +
										"\r\n" +
										"\r\n\t\t\tm_Database.Initialize();" +
										"\r\n\t\t\tm_Database.BeginTransaction();" +
										"\r\n\t\t\tm_ContactsUpdate = new ContactsUpdate(m_Database);" +
										"\r\n\t\t}" +
										"\r\n" +
										"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
										"\r\n\t\t/// Method <cTearDownSetUp</c>" +
										"\r\n\t\t/// <summary>" +
										"\r\n\t\t/// function that is called just after each test method is called." +
										"\r\n\t\t/// </summary>" +
										"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
										"\r\n\t\t[TearDown]" +
										"\r\n\t\tpublic void TearDown()" +
										"\r\n\t\t{" +
										"\r\n\t\t\tm_Database.CommitTransaction();" +
										"\r\n\t\t\tm_Database.ShutDown();" +
										"\r\n\t\t}";

			return TestFileContents;
		}

		private string AppendUpdateQuery(
			string ColumnName,
			string UpdateQuery)
		{
			UpdateQuery += "\t\t\tif (null != " + ColumnName + ")\r\n" +
				"\t\t\t{\r\n" +
				"\t\t\t\tif (false == IsFirst)\r\n" +
				"\t\t\t\t{\r\n" +
				"\t\t\t\tSqlQuery += \", \";\r\n" +
				"\t\t\t\t}\r\n" +
				"\t\t\t\telse\r\n" +
				"\t\t\t\t{\r\n" +
				"\t\t\t\tIsFirst = false;\r\n" +
				"\t\t\t\t}\r\n" +
				"\t\t\t\tSqlQuery += \"[" + ColumnName + "]='\" + " + ColumnName + " + \"'\";\r\n" +
				"\t\t\t}\r\n";

			return UpdateQuery;
		}

		private ColumnTypes GetColumnType(
			string Column)
		{
			ColumnTypes ColumnType = ColumnTypes.String;

			if (Column.Contains("DATETIME"))
			{
				ColumnType = ColumnTypes.DateTime;
			}
			else if (Column.Contains("MEMO"))
			{
				ColumnType = ColumnTypes.Memo;
			}
			else if (Column.Contains("INTEGER"))
			{
				ColumnType = ColumnTypes.Number;
			}
			else if (Column.Contains("YESNO"))
			{
				ColumnType = ColumnTypes.YesNo;
			}

			return ColumnType;
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

		/////////////////////////////////////////////////////////////////////
		/// Method <c>CreateUpdateStatments</c>
		/// <summary>
		/// Crates files that have the  update functions and the test
		/// functions for the given schema file.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public void CreateUpdateStatments(
			string SchemaFile,
			string UpdateFile,
			string TestFile)
		{
			try
			{
				if (File.Exists(SchemaFile))
				{
					string FileContents = Utils.GetFileContents(SchemaFile);

					string[] Queries = GetTableDefinitions(FileContents);

					//set up a streamwriter for adding text
					StreamWriter StreamWriterObject = new StreamWriter(UpdateFile, false, Encoding.Default);
					string NewFileContents = GetFileHeader(); ;

					//set up a streamwriter for adding text
					StreamWriter TestFileStreamWriter = new StreamWriter(TestFile, false, Encoding.Default);
					string TestFileContents = GetTestFileHeader();

					foreach (string Table in Queries)
					{
						string TableName = GetTableNameFromTableDefinitions(Table);
						string UpdateQuery = "UPDATE " + TableName + " SET \";\r\n\r\n";

						string FunctionHeader = GetFunctionHeader(TableName);

						string TestFunction = "\r\n\r\n\t\t/////////////////////////////////////////////////////////////////////" +
							"\r\n\t\t/// Method <c>" + TableName + "</c>" +
							"\r\n\t\t/// <summary>" +
							"\r\n\t\t/// Tests Updating a " + TableName + " record" +
							"\r\n\t\t/// </summary>" +
							"\r\n\t\t/////////////////////////////////////////////////////////////////////" +
							"\r\n\t\t[Test]" +
							"\r\n\t\tpublic void " + TableName + "()" + "\r\n\t\t{" +
							"\r\n\t\t\tbool ReturnCode = m_ContactsUpdate." + TableName + "(";

						string ColumnsInfo = GetColumnsInfo(Table);
						string[] TableColumnParts = ColumnsInfo.Split(new char[] { '[' });

						bool First = true;
						//uint Index = 0;
						string PrimaryKey = null;

						for (uint Index = 1; Index < TableColumnParts.Length; Index++)
						//foreach (string Column in TableColumnParts)
						{
							string Column = TableColumnParts[Index];
							string[] ColumnParts = Column.Split(new char[] { ']' });

							if (1 == Index)
							{
								PrimaryKey = ColumnParts[0];
							}
							// the first item is junk and
							// the second item is the primary key,
							// which we will never update
							//else if (1 < Index)
							else
							{
								string ColumnName = ColumnParts[0];
								if (true == First)
								{
									UpdateQuery = AppendUpdateQuery(ColumnName, UpdateQuery);
									First = false;
								}
								else
								{
									UpdateQuery = AppendUpdateQuery(ColumnName, UpdateQuery);
									FunctionHeader += ",";
									TestFunction += ", ";
								}

								FunctionHeader += "\r\n\t\t\tstring " + ColumnName;

								ColumnTypes ColumnType = GetColumnType(ColumnParts[1]);

								if (ColumnTypes.DateTime == ColumnType)
								{
									TestFunction += "\"" + DateTime.Now + "\"";
								}
								else if ((ColumnTypes.Number == ColumnType) || (ColumnTypes.YesNo == ColumnType))
								{
									TestFunction += "\"1\"";
								}
								else
								{
									TestFunction += "\"" + ColumnName + "\"";
								}
							}

							if (ColumnParts[1].Contains("CONSTRAINT"))
							{
								break;
							}
						}

						FunctionHeader += ",\r\n\t\t\tint PrimaryKey)\r\n\t\t{\r\n\t\t\tstring SqlQuery = \"";
						//UpdateQuery += "\r\n";
						TestFunction += ",1);";

						string PrimaryKeyClause = "\r\n\t\t\tSqlQuery += \" WHERE " + PrimaryKey + "=\" + PrimaryKey;\r\n";
						NewFileContents += FunctionHeader + UpdateQuery + PrimaryKeyClause + "\r\n" +
							"\t\t\tbool ReturnedCode = m_Database.UpdateCommand(SqlQuery);" +
							"\r\n\r\n\t\t\treturn ReturnedCode;" +
							"\r\n\t\t}";

						TestFileContents += TestFunction + "\r\n" +
							"\r\n\t\t\tAssert.IsTrue(ReturnCode);" +
							"\r\n\t\t}";
					}

					NewFileContents += "\r\n\t}\r\n}\r\n";

					StreamWriterObject.Write(NewFileContents);

					StreamWriterObject.Close();

					TestFileContents += "\r\n\t}\r\n}\r\n";

					TestFileStreamWriter.Write(TestFileContents);

					TestFileStreamWriter.Close();
				}
			}
			catch (Exception Ex)
			{
				Console.WriteLine("Exception: " + Ex.ToString());
			}
		}

		private bool ExportDataTableToCsv(
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

		private DataTable GetTableNames(
			CoreDatabase Database)
		{
			DataTable Tables = Database.GetSchemaTable();

			return Tables;
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>ExportToCsv</c>
		/// <summary>
		/// Export all tables to similiarly named csv files
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public bool ExportToCsv(
			string DatebaseFile,
			string CsvPath)
		{
			bool ReturnCode = false;

			// Open the database
			CoreDatabase Database = new CoreDatabase("DatabaseLibaryNET",
													"Microsoft.Jet.OLEDB.4.0",
													DatebaseFile);

			ReturnCode = Database.Initialize();

			if (true == ReturnCode)
			{
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

				Database.ShutDown();
			}

			return ReturnCode;
		}

		private string MakePrivledgedConnectString(
			string MdbFile)
		{
			return @"Provider=Microsoft.Jet.OLEDB.4.0;Password="""";User ID=Admin;"
					+ "Data Source=" + MdbFile + @";Mode=Share Deny None;Extended Properties="""";Jet OLEDB:System database="""";"
					+ @"Jet OLEDB:Registry Path="""";Jet OLEDB:Database Password="""";Jet OLEDB:Engine Type=5;"
					+ @"Jet OLEDB:Database Locking Mode=1;Jet OLEDB:Global Partial Bulk Ops=2;Jet OLEDB:Global Bulk Transactions=1;"
					+ @"Jet OLEDB:New Database Password="""";Jet OLEDB:Create System Database=False;Jet OLEDB:Encrypt Database=False;"
					+ @"Jet OLEDB:Don't Copy Locale on Compact=False;Jet OLEDB:Compact Without Replica Repair=False;Jet OLEDB:SFP=False";
		}

		/////////////////////////////////////////////////////////////////////
		/// Method <c>ExportSchema</c>
		/// <summary>
		/// Export all tables to similiarly named csv files
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