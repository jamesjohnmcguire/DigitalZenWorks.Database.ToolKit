/////////////////////////////////////////////////////////////////////////////
// $Id: $
//
// Copyright © 2006-2016 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using Common.Logging;
using DigitalZenWorks.Common.Utils;
using System;
using System.Data;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;

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
		private static readonly ResourceManager stringTable =
			new ResourceManager("DigitalZenWorks.Common.DatabaseLibrary",
			Assembly.GetExecutingAssembly());

		/////////////////////////////////////////////////////////////////////
		/// Method <c>ExportSchema</c>
		/// <summary>
		/// Export all tables to similarly named csv files
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
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
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is InvalidOperationException)
			{
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception.Message));
			}
			catch
			{
				throw;
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
			string ColumnsInfo = null;

			if (!string.IsNullOrWhiteSpace(dataDefinition))
			{
				int SplitIndex = dataDefinition.IndexOf("(",
					StringComparison.Ordinal) + 1;
				ColumnsInfo = dataDefinition.Substring(SplitIndex);
			}

			return ColumnsInfo;
		}

		/// <summary>
		/// Returns the column type
		/// </summary>
		/// <param name="column"></param>
		/// <returns></returns>
		public static ColumnType GetColumnType(
			string column)
		{
			string[] columnTypeComareKeys = { "autonumber", "identity",
				"autoincrement", "bigint", "longvarbinary", "longvarchar",
				"varbinary", "varchar", "binary", "bit", "longblob",
				"mediumblob", "blob", "boolean", "byte", "nvarchar", "char",
				"currency", "cursor", "smalldatetime", "smallint",
				"smallmoney", "datetime2", "datetimeoffset", "datetime",
				"date", "decimal", "double", "hyperlink", "enum", "float",
				"image", "integer", "mediumint", "tinyint", "int",
				"javaobject", "longtext", "long", "lookupwizard", "mediumtext",
				"memo", "money", "nchar", "ntext", "number", "numeric",
				"oleobject", "ole", "real", "set", "single", "sqlvariant",
				"string", "table", "tinytext", "text", "timestamp", "time",
				"uniqueidentifier", "xml", "year", "yesno" };

		ColumnType[] types = { ColumnType.AutoNumber, ColumnType.Identity,
			ColumnType.Identity, ColumnType.BigInt, ColumnType.LongVarBinary,
			ColumnType.LongVarChar, ColumnType.VarBinary, ColumnType.VarChar,
			ColumnType.Binary, ColumnType.Bit, ColumnType.LongBlob,
			ColumnType.MediumBlob, ColumnType.Blob, ColumnType.Boolean,
			ColumnType.Byte, ColumnType.NVarChar, ColumnType.Char,
			ColumnType.Currency, ColumnType.Cursor, ColumnType.SmallDateTime,
			ColumnType.SmallInt, ColumnType.SmallMoney, ColumnType.DateTime2,
			ColumnType.DateTimeOffset, ColumnType.DateTime, ColumnType.Date,
			ColumnType.Decimal, ColumnType.Double, ColumnType.Hyperlink,
			ColumnType.Enum, ColumnType.Float, ColumnType.Image,
			ColumnType.Integer, ColumnType.MediumInt, ColumnType.TinyInt,
			ColumnType.Int, ColumnType.JavaObject, ColumnType.LongText,
			ColumnType.Long, ColumnType.LookupWizard, ColumnType.MediumText,
			ColumnType.Memo, ColumnType.Money, ColumnType.NChar,
			ColumnType.NText, ColumnType.Number, ColumnType.Numeric,
			ColumnType.OleObject, ColumnType.Ole, ColumnType.Real,
			ColumnType.Set, ColumnType.Single, ColumnType.SqlVariant,
			ColumnType.String, ColumnType.Table, ColumnType.TinyText,
			ColumnType.Text, ColumnType.Timestamp, ColumnType.Time,
			ColumnType.UniqueIdentifier, ColumnType.Xml, ColumnType.Year,
			ColumnType.Boolean };

			ColumnType columnType = ColumnType.Other;

			for(int index=0; index < columnTypeComareKeys.Length; index++)
			{
				if (CompareColumnType(column, columnTypeComareKeys[index],
					types[index], ref columnType))
				{
					break;
				}
			}

			return columnType;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="relationships"></param>
		/// <returns></returns>
		public static ForeignKey[] GetForeignKeyRelationships(
			Relationship[] relationships)
		{
			ForeignKey[] keys = null;

			if (null != relationships)
			{
				int count = 0;
				keys = new ForeignKey[relationships.Length];

				// Add foreign keys to table, using relationships
				foreach (Relationship relationship in relationships)
				{
					ForeignKey foreignKey =
						GetForeignKeyRelationship(relationship);

					keys[count] = foreignKey;
					count++;
				}
			}

			return keys;
		}

		/// <summary>
		/// Gets a list of relationships
		/// </summary>
		/// <param name="oleDbSchema"></param>
		/// <param name="tableName"></param>
		/// <returns></returns>
		public static ArrayList GetRelationships(OleDbSchema oleDbSchema,
			string tableName)
		{
			ArrayList relationships = null;

			if ((null != oleDbSchema) &&
				(!string.IsNullOrWhiteSpace(tableName)))
			{
				relationships = new System.Collections.ArrayList();

				DataTable foreignKeyTable = oleDbSchema.GetForeignKeys(tableName);

				foreach (DataRow foreignKey in foreignKeyTable.Rows)
				{
					Relationship relationship = GetRelationship(foreignKey);
					relationships.Add(relationship);
				}

			}

			return relationships;
		}

		/// <summary>
		/// GetTableDefinitions - returns an array of table definitions
		/// </summary>
		/// <param name="tableDefinitionsFile"></param>
		/// <returns></returns>
		public static string[] GetTableDefinitions(
			string tableDefinitionsFile)
		{
			string[] queries = null;

			if (!string.IsNullOrWhiteSpace(tableDefinitionsFile))
			{
				string[] stringSeparators = new string[] { "\r\n\r\n" };
				queries = tableDefinitionsFile.Split(stringSeparators,
					32000, StringSplitOptions.RemoveEmptyEntries);
			}

			return queries;
		}

		/// <summary>
		/// GetTableName - returns the name of the table
		/// </summary>
		/// <param name="dataDefinition"></param>
		/// <returns></returns>
		public static string GetTableName(string dataDefinition)
		{
			string TableName = null;

			if (!string.IsNullOrWhiteSpace(dataDefinition))
			{
				string[] TableParts = dataDefinition.Split(new char[] { '(' });

				string[] TableNameParts =
					TableParts[0].Split(new char[] { '[', ']', '`' });

				if (TableNameParts.Length > 1)
				{
					TableName = TableNameParts[1];
				}
			}

			return TableName;
		}

		/// <summary>
		/// Creates a mdb file with the given schema.
		/// </summary>
		/// <param name="schemaFile"></param>
		/// <param name="databaseFile"></param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage(
			"Microsoft.Reliability",
			"CA2000:Dispose objects before losing scope")]
		public static bool ImportSchema(string schemaFile, string databaseFile)
		{
			bool successCode = false;
			try
			{
				if (File.Exists(schemaFile))
				{
					string provider = "Microsoft.Jet.OLEDB.4.0";
					if (Environment.Is64BitProcess)
					{
						provider = "Microsoft.ACE.OLEDB.12.0";
					}
					string fileContents =
						FileUtils.GetFileContents(schemaFile);

					string[] StringSeparators = new string[] { "\r\n\r\n" };
					string[] Queries = fileContents.Split(StringSeparators,
						32000, StringSplitOptions.RemoveEmptyEntries);

					using (DataStorage database = new DataStorage(provider,
						databaseFile))
					{
					foreach (string SqlQuery in Queries)
					{
							log.Info(CultureInfo.InvariantCulture, m => m(
								stringTable.GetString("COMMAND") + SqlQuery));

							database.ExecuteNonQuery(SqlQuery);
					}

					successCode = true;
				}
			}
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is FileNotFoundException ||
				exception is DirectoryNotFoundException ||
				exception is IOException ||
				exception is OutOfMemoryException)
			{
				log.Error(CultureInfo.InvariantCulture, m => m(
					stringTable.GetString("EXCEPTION") + exception.Message));
			}
			catch
			{
				throw;
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

			if (!string.IsNullOrWhiteSpace(field))
			{
				if ((field.Contains("Time")) || (field.Contains("time")))
				{
					returnCode = true;
				}
			}

			return returnCode;
		}

		private static bool CompareColumnType(string column, string nameCheck,
			ColumnType columnType, ref ColumnType columnTypeOut)
		{
			bool found = false;

			if ((column.ToUpperInvariant().Contains(nameCheck)) ||
				(column.ToUpperInvariant().Equals(nameCheck)))
			{
				columnTypeOut = columnType;
				found = true;
			}

			return found;
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
					string flags = row["COLUMN_FLAGS"].ToString();

					if (Int32.Parse(flags, CultureInfo.InvariantCulture) > 127)
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
				string maxLength = row["CHARACTER_MAXIMUM_LENGTH"].ToString();

				column.Length =
					Int32.Parse(maxLength, CultureInfo.InvariantCulture);
			}

			if (row["IS_NULLABLE"].ToString() == "True")
			{
				column.Nullable = true;
			}

			if (row["COLUMN_HASDEFAULT"].ToString() == "True")
			{
				column.DefaultValue = row["COLUMN_DEFAULT"].ToString();
			}

			string position = row["ORDINAL_POSITION"].ToString();
			column.Position =
				Int32.Parse(position, CultureInfo.InvariantCulture);

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

		private static Hashtable GetSchema(string databaseFile)
		{
			Hashtable tables = null;

			using (OleDbSchema oleDbSchema = new OleDbSchema(databaseFile))
			{
				tables = new System.Collections.Hashtable();
			ArrayList relationships = new System.Collections.ArrayList();

				DataTable tableNames = oleDbSchema.TableNames;

			foreach (DataRow row in tableNames.Rows)
			{
				string tableName = row["TABLE_NAME"].ToString();

				Table table = new Table(tableName);

					log.Info(CultureInfo.InvariantCulture,
						m => m("Getting Columns for " + tableName));
					DataTable dataColumns =
						oleDbSchema.GetTableColumns(tableName);

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
					if (!string.IsNullOrWhiteSpace(table.PrimaryKey))
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
						Relationship relationship =
							GetRelationship(foreignKey);

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

					((Table)tables[name]).ForeignKeys.Add(foreignKey);
			}
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
					sql += String.Format(CultureInfo.InvariantCulture,
						" VARCHAR({0})", column.Length);
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

			if (!string.IsNullOrWhiteSpace(column.DefaultValue))
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
				sql = String.Format(CultureInfo.InvariantCulture,
					"CONSTRAINT `{0}` FOREIGN KEY (`{1}`) REFERENCES `{2}`",
					foreignKey.Name, foreignKey.ColumnName,
					foreignKey.ParentTable);
			}
			else
			{
				sql = String.Format(CultureInfo.InvariantCulture,
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

			sql += String.Format(CultureInfo.InvariantCulture,
				"CREATE TABLE `{0}` ({1}", table.Name, Environment.NewLine);

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

			if (!string.IsNullOrWhiteSpace(table.PrimaryKey))
			{
				sql += String.Format(CultureInfo.InvariantCulture,
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

			sql += String.Format(CultureInfo.InvariantCulture, "{0});{0}",
				Environment.NewLine);

			return sql;
		}

	} // End class
} // End Namespace
