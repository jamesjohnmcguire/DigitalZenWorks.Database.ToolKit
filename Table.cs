﻿/////////////////////////////////////////////////////////////////////////////
// $Id: $
//
// Copyright (c) 2006-2014 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using DigitalZenWorks.Common.DatabaseLibrary;
using System;
using System.Collections;
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
		private Hashtable columns;
		private ArrayList foreignKeys;
		private string name = string.Empty;
		private string primaryKey = string.Empty;
		private static readonly ResourceManager stringTable =
			new ResourceManager("DigitalZenWorks.Common.DatabaseLibrary",
			Assembly.GetExecutingAssembly());

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents the columns
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public Hashtable Columns
		{
			get { return columns; }
			set { columns = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents the foreign keys
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public ArrayList ForeignKeys
		{
			get { return foreignKeys; }
			set { foreignKeys = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Represents a table name
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
		/// Default constructor
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public Table()
		{
			ForeignKeys = new System.Collections.ArrayList();
			Columns = new System.Collections.Hashtable();
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor - with name
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
		/// Add a column
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
		/// Adds a foreign key
		/// </summary>
		/// <param name="foreignKey"></param>
		/////////////////////////////////////////////////////////////////////
		public void AddForeignKey(ForeignKey foreignKey)
		{
			ForeignKeys.Add(foreignKey);
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Writes out the table information
		/// </summary>
		/// <returns>DataTable</returns>
		/////////////////////////////////////////////////////////////////////
		public void DumpTable()
		{
			Console.WriteLine(stringTable.GetString("TABLE") + Name);
			foreach (DictionaryEntry column in Columns)
			{
				Console.WriteLine(stringTable.GetString("TABDASH") +
					((Column)column.Value).Name);
			}

			Console.WriteLine(stringTable.GetString("PRIMARYKEY") +
				PrimaryKey);

			foreach (ForeignKey foreignKey in ForeignKeys)
			{
				string output = string.Format("{0} {1} {2}", foreignKey.Name,
					foreignKey.ColumnName, foreignKey.ParentTable);
				Console.WriteLine(stringTable.GetString("FOREIGNKEY") +
					output);
			}
		}
	}
}
