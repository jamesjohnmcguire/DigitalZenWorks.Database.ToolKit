/////////////////////////////////////////////////////////////////////////////
// <copyright file="Table.cs" company="James John McGuire">
// Copyright © 2006 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using Common.Logging;
using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Reflection;
using System.Resources;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Represents a database table.
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class Table
	{
		private Hashtable columns = new System.Collections.Hashtable();
		private ArrayList foreignKeys = new System.Collections.ArrayList();

		/// <summary>
		/// Diagnostics object.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string name = string.Empty;
		private string primaryKey = string.Empty;
		private static readonly ResourceManager stringTable = new
			ResourceManager(
			"DatabaseLibraryNet.Resources",
			Assembly.GetExecutingAssembly());

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents the columns.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public Hashtable Columns
		{
			get { return columns; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents the foreign keys.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public ArrayList ForeignKeys
		{
			get { return foreignKeys; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents a table name.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents the primary key
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public string PrimaryKey
		{
			get { return primaryKey; }
			set { primaryKey = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Default constructor.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public Table()
		{
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor - with name.
		/// </summary>
		/// <param name="name"></param>
		/////////////////////////////////////////////////////////////////////
		public Table(string name)
			: this()
		{
			Name = name;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Add a column.
		/// </summary>
		/// <param name="column"></param>
		/////////////////////////////////////////////////////////////////////
		public void AddColumn(Column column)
		{
			if (null != column)
			{
				Columns.Add(column.Name, column);
			}
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Writes out the table information.
		/// </summary>
		/// <returns>DataTable</returns>
		/////////////////////////////////////////////////////////////////////
		public string Dump()
		{
			log.Info(
				CultureInfo.InvariantCulture,
				m => m(stringTable.GetString("TABLE") + Name));
			string output = stringTable.GetString("TABLE") + Name +
				Environment.NewLine;

			foreach (DictionaryEntry column in Columns)
			{
				log.Info(
					CultureInfo.InvariantCulture,
					m => m(stringTable.GetString("TABDASH") +
					((Column)column.Value).Name));
				output += stringTable.GetString("TABDASH") +
					((Column)column.Value).Name + Environment.NewLine;
			}

			log.Info(
				CultureInfo.InvariantCulture,
				m => m(stringTable.GetString("PRIMARYKEY") + PrimaryKey));
			output += stringTable.GetString("PRIMARYKEY") + PrimaryKey +
				Environment.NewLine;

			foreach (ForeignKey foreignKey in ForeignKeys)
			{
				string format = string.Format(
					CultureInfo.InvariantCulture,
					"{0} {1} {2}",
					foreignKey.Name,
					foreignKey.ColumnName,
					foreignKey.ParentTable);

				log.Info(
					CultureInfo.InvariantCulture,
					m => m(stringTable.GetString("FOREIGNKEY") + format));
				output += stringTable.GetString("FOREIGNKEY") + format +
					Environment.NewLine;
			}

			return output;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Writes out the table information.
		/// </summary>
		/// <returns>DataTable</returns>
		/////////////////////////////////////////////////////////////////////
		public static string Dump(DataTable table)
		{
			string output = string.Empty;

			if (null != table)
			{
				log.Info(
					CultureInfo.InvariantCulture,
					m => m(stringTable.GetString("TABLE") + table.TableName));
				output = stringTable.GetString("TABLE") +
					table.TableName + Environment.NewLine;

				foreach (DataColumn column in table.Columns)
				{
					log.Info(CultureInfo.InvariantCulture, m => m(
						stringTable.GetString("TABDASH") + column.ColumnName));
					output += stringTable.GetString("TABDASH") +
						column.ColumnName + Environment.NewLine;
				}
			}

			log.Info(CultureInfo.InvariantCulture, m => m(
				stringTable.GetString("PRIMARYKEY") + table.PrimaryKey));
			output += stringTable.GetString("PRIMARYKEY") + table.PrimaryKey +
				Environment.NewLine;

			return output;
		}
	}
}
