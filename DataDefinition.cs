/////////////////////////////////////////////////////////////////////////////
// $Id: DataDefinition.cs 38 2015-05-27 14:24:29Z JamesMc $
//
// Copyright (c) 2006-2015 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using Common.Logging;
using DigitalZenWorks.Common.Utils;
using System;
using System.Data;
using System.Collections;
using System.Globalization;
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
		/// <summary>
		/// Diagnostics object
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger
			(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/////////////////////////////////////////////////////////////////////
		/// Method <c>ExportSchema</c>
		/// <summary>
		/// Export all tables to similarly named csv files
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public static bool ExportSchema(string databaseFile, string schemaFile)
		{
			bool successCode = false;

			try
			{
				Hashtable tables = GetSchema(databaseFile);

				string schemaText = string.Empty;

				ArrayList list = OrderTable(tables);

				foreach (string table in list)
				{
					schemaText += WriteSql((Table)tables[table]) +
						Environment.NewLine;
				}

				FileUtils.SaveFile(schemaText, schemaFile);

				successCode = true;
			}
			catch(Exception ex)
			{
				log.Error(CultureInfo.InvariantCulture,
					m => m("Exception: " + ex.Message));
			}

			return successCode;
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
		public static bool ImportSchema(string schemaFile, string databaseFile)
		{
			bool successCode = false;
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

					DataStorage Database = new DataStorage(provider,
						databaseFile);

					foreach (string SqlQuery in Queries)
					{
						Database.ExecuteNonQuery(SqlQuery);
					}

					Database.Shutdown();

					successCode = true;
				}
			}
			catch (Exception Ex)
			{
				Console.WriteLine("Exception: " + Ex.ToString());
			}

			return successCode;
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

		private static void DumpTable(DataTable table)
		{
			foreach (DataRow row in table.Rows)
			{
				foreach (DataColumn column in table.Columns)
				{
					Console.WriteLine(column.ColumnName + " = " +
						row[column].ToString());
				}
				Console.WriteLine();
			}
		}

		private static Column FormatColumnFromDataRow(DataRow row)
		{
			Column column = new Column();
			column.Name = row["COLUMN_NAME"].ToString();

			switch ((int)row["DATA_TYPE"])
			{
				case 3:	// Number
				{
					column.Type = (int)ColumnType.Number;
					break;
				}

				case 130:  // String
				{
					if (Int32.Parse(row["COLUMN_FLAGS"].ToString()) > 127)
					{
						column.Type = (int)ColumnType.Memo;
					}
					else
					{
						column.Type = (int)ColumnType.String;
					}
					break;
				}

				case 7:  // Date
				{
					column.Type = (int)ColumnType.DateTime;
					break;
				}

				case 6:  // Currency
				{
					column.Type = (int)ColumnType.Currency;
					break;
				}

				case 11:  // Yes/No
				{
					column.Type = (int)ColumnType.YesNo;
					break;
				}

				case 128:  // OLE
				{
					column.Type = (int)ColumnType.Ole;
					break;
				}
			}

			if (!row.IsNull("CHARACTER_MAXIMUM_LENGTH"))
			{
				column.Length =
					Int32.Parse(row["CHARACTER_MAXIMUM_LENGTH"].ToString());
			}

			if (row["IS_NULLABLE"].ToString() == "True")
			{
				column.Nullable = true;
			}

			if (row["COLUMN_HASDEFAULT"].ToString() == "True")
			{
				column.DefaultValue = row["COLUMN_DEFAULT"].ToString();
			}

			column.Position = Int32.Parse(row["ORDINAL_POSITION"].ToString());

			return column;
		}

		private static ForeignKey GetForeignKeyRelationship(
			Relationship relationship)
		{
			ForeignKey foreignKey = new ForeignKey(relationship.Name,
				relationship.ChildTableCol, relationship.ParentTable,
				relationship.ParentTableCol, relationship.OnDeleteCascade,
				relationship.OnUpdateCascade);

			return foreignKey;
		}

		private static ForeignKey[] GetForeignKeyRelationships(
			Relationship[] relationships)
		{
			int count = 0;
			ForeignKey[] keys = new ForeignKey[relationships.Length];

			// Add foreign keys to table, using relationships
			foreach (Relationship relationship in relationships)
			{
				ForeignKey foreignKey =
					GetForeignKeyRelationship(relationship);

				keys[count] = foreignKey;
				count++;
			}

			return keys;
		}

		private static Relationship GetRelationship(DataRow foreignKey)
		{
			Relationship relationship = new Relationship();
				relationship.Name = foreignKey["FK_NAME"].ToString();
				relationship.ParentTable =
					foreignKey["PK_TABLE_NAME"].ToString();
				relationship.ParentTableCol =
					foreignKey["PK_COLUMN_NAME"].ToString();
				relationship.ChildTable =
					foreignKey["FK_TABLE_NAME"].ToString();
				relationship.ChildTableCol =
					foreignKey["FK_COLUMN_NAME"].ToString();

			if (foreignKey["UPDATE_RULE"].ToString() != "NO ACTION")
				relationship.OnUpdateCascade = true;

			if (foreignKey["DELETE_RULE"].ToString() != "NO ACTION")
				relationship.OnDeleteCascade = true;

			return relationship;
		}

		private static ArrayList GetRelationships(OleDbSchema oleDbSchema,
			string tableName)
		{
			ArrayList relationships = new System.Collections.ArrayList();

			DataTable foreignKeyTable = oleDbSchema.GetForeignKeys(tableName);

			foreach (DataRow foreignKey in foreignKeyTable.Rows)
			{
				Relationship relationship = GetRelationship(foreignKey);
				relationships.Add(relationship);
			}

			return relationships;
		}

		private static Hashtable GetSchema(string databaseFile)
		{
			OleDbSchema oleDbSchema = new OleDbSchema(databaseFile);

			Hashtable tables = new System.Collections.Hashtable();
			ArrayList relationships = new System.Collections.ArrayList();

			DataTable tableNames = oleDbSchema.GetTableNames();

			foreach (DataRow row in tableNames.Rows)
			{
				string tableName = row["TABLE_NAME"].ToString();

				Table table = new Table(tableName);

			    Console.WriteLine("Getting Columns for " + tableName);
			    DataTable dataColumns = oleDbSchema.GetTableColumns(tableName);

				foreach (DataRow dataColumn in dataColumns.Rows)
				{
					Column column = FormatColumnFromDataRow(dataColumn);

					table.AddColumn(column);
				}

				// Get primary key
				DataTable primary_key_table =
					oleDbSchema.GetPrimaryKeys(tableName);

				foreach (DataRow pkrow in primary_key_table.Rows)
				{
					table.PrimaryKey = pkrow["COLUMN_NAME"].ToString();
				}

				// If PK is an integer change type to AutoNumber
				if (table.PrimaryKey != "")
				{
					if (((Column)table.Columns[table.PrimaryKey]).Type ==
						(int)ColumnType.Number)
					{
						((Column)table.Columns[table.PrimaryKey]).Type =
							(int)ColumnType.AutoNumber;
					}
				}

				DataTable foreignKeyTable =
					oleDbSchema.GetForeignKeys(tableName);

				foreach (DataRow foreignKey in foreignKeyTable.Rows)
				{
					Relationship relationship = GetRelationship(foreignKey);

					relationships.Add(relationship);
				}
	
				tables.Add(table.Name, table);
			}

			// Add foreign keys to table, using relationships
			foreach (Relationship relationship in relationships)
			{
				string name = relationship.ChildTable;

				ForeignKey foreignKey =
					GetForeignKeyRelationship(relationship);

				((Table)tables[name]).AddForeignKey(foreignKey);
			}

			return tables;
		}

		// Return an array list in the order that the tables need to be added
		// to take dependencies into account
		private static ArrayList OrderTable(Hashtable hashTable)
		{
			Hashtable list = new Hashtable();
			ArrayList dependencies = new ArrayList();

			foreach (DictionaryEntry entry in hashTable)
			{
				string Name = (string)entry.Key;
				Table table = (Table)entry.Value;
				foreach (ForeignKey foreignKeys in table.ForeignKeys)
				{
					dependencies.Add(foreignKeys.ParentTable);
				}

				list.Add(Name, new ArrayList(dependencies));
				dependencies.Clear();
			}

			return TopologicalSort(list);
		}

		/// <summary>
		/// Performs a topological sort on a list with dependencies
		/// </summary>
		/// <param name="table">A table to be sorted with the structure
		/// Object name, ArrayList dependencies.</param>
		/// <returns>A sorted arraylist.</returns>
		private static ArrayList TopologicalSort(Hashtable table)
		{
			ArrayList sortedList = new ArrayList();
			object key;
			ArrayList dependencies;

			while (sortedList.Count < table.Count)
			{
				foreach (DictionaryEntry entry in table)
				{
					key = entry.Key;
					dependencies = (ArrayList)entry.Value;

					// No dependencies, add to start of table.
					if (dependencies.Count == 0)
					{
						if (!sortedList.Contains(key))
						{
							log.Info(CultureInfo.InvariantCulture,
								m => m("Adding: (ND) " + key.ToString()));
							sortedList.Insert(0, key);
						}
						continue;
					}

					bool allDependenciesExist = false;
					int lastDependency = 0;

					foreach (object dependency in dependencies)
					{
						if (sortedList.Contains(dependency))
						{
							allDependenciesExist = true;
							if (sortedList.IndexOf(dependency) >
								lastDependency)
							{
								lastDependency =
									sortedList.IndexOf(dependency);
							}
						}
						else
						{
							allDependenciesExist = false;
							break;
						}
					}

					// All dependencies have been added, add object at
					// location of last dependency.
					if (allDependenciesExist)
					{
						if (!sortedList.Contains(key))
						{
							log.Info(CultureInfo.InvariantCulture,
								m => m("Adding: (D) " + key.ToString()));
							sortedList.Add(key);
						}
					}
				}
			}

			return sortedList;
		}

		// Write the SQL for a column
		private static string WriteColumnSql(Column column)
		{
			string sql = string.Empty;

			sql += "`" + column.Name + "`";

			switch (column.Type)
			{
				case (int)ColumnType.Number:
				case (int)ColumnType.AutoNumber:
				{
					sql += " INTEGER";
					break;
				}
				case (int)ColumnType.String:
				{
					sql += String.Format(" VARCHAR({0})", column.Length);
					break;
				}

				case (int)ColumnType.Memo:
				{
					sql += " MEMO";
					break;
				}

				case (int)ColumnType.DateTime:
				{
					sql += " DATETIME";
					break;
				}

				case (int)ColumnType.Currency:
				{
					sql += " CURRENCY";
					break;
				}

				case (int)ColumnType.Ole:
				{
					sql += " OLEOBJECT";
					break;
				}

				case (int)ColumnType.YesNo:
				{
					sql += " OLEOBJECT";
					break;
				}
			}

			if (column.Unique)
			{
				sql += " UNIQUE";
			}

			if (!column.Nullable)
			{
				sql += " NOT NULL";
			}

			if (column.Type == (int)ColumnType.AutoNumber)
			{
				sql += " IDENTITY";
			}

			if (!(column.DefaultValue == ""))
			{
				sql += " DEFAULT " + column.DefaultValue;
			}

			return sql;
		}

		// Write the SQL for a Foreign Key constraint
		private static string WriteForeignKeySql(ForeignKey foreignKey)
		{
			string sql = string.Empty;

			if (foreignKey.ColumnName == foreignKey.ParentTableColumn)
			{
				sql = String.Format(
					"CONSTRAINT `{0}` FOREIGN KEY (`{1}`) REFERENCES `{2}`",
					foreignKey.Name, foreignKey.ColumnName,
					foreignKey.ParentTable);
			}
			else
			{
				sql = String.Format(
					"CONSTRAINT `{0}` FOREIGN KEY (`{1}`) " +
					"REFERENCES `{2}` (`{3}`)", foreignKey.Name,
					foreignKey.ColumnName, foreignKey.ParentTable,
					foreignKey.ParentTableColumn);
			}

			if (foreignKey.CascadeOnDelete)
			{
				sql += " ON DELETE CASCADE";
			}

			if (foreignKey.CascadeOnUpdate)
			{
				sql += " ON UPDATE CASCADE";
			}

			return sql;
		}

		// Take a Table structure and output the SQL commands
		private static string WriteSql(Table table)
		{
			string sql = string.Empty;

			sql += String.Format("CREATE TABLE `{0}` ({1}", table.Name,
				Environment.NewLine);

			// Sort Columns into ordinal positions
			System.Collections.SortedList columns =
				new System.Collections.SortedList();

			foreach (DictionaryEntry entry in table.Columns)
			{
				Column column = (Column)entry.Value;
				columns.Add(column.Position, column);
			}

			foreach (DictionaryEntry entry in columns)
			{
				sql += "\t" + WriteColumnSql((Column)entry.Value) + "," +
					Environment.NewLine;
			}

			if (!(table.PrimaryKey == ""))
			{
				sql += String.Format(
					"\tCONSTRAINT PrimaryKey PRIMARY KEY (`{0}`),{1}",
					table.PrimaryKey, Environment.NewLine);
			}

			foreach (ForeignKey foreignKey in table.ForeignKeys)
			{
				sql += "\t" + WriteForeignKeySql(foreignKey) + "," +
					Environment.NewLine;
			}

			// Remove trailing ','
			sql = sql.Remove(sql.Length - 3, 3);

			sql += String.Format("{0});{0}", Environment.NewLine);

			return sql;
		}

	} // End class
} // End Namespace