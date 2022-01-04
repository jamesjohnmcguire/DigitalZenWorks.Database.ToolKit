/////////////////////////////////////////////////////////////////////////////
// <copyright file="Table.cs" company="James John McGuire">
// Copyright © 2006 - 2022 James John McGuire. All Rights Reserved.
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
		/// <summary>
		/// Diagnostics object.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ResourceManager StringTable = new
			ResourceManager(
			"DigitalZenWorks.Common.DatabaseLibrary.Resources",
			Assembly.GetExecutingAssembly());

		private readonly Hashtable columns = new System.Collections.Hashtable();
		private readonly ArrayList foreignKeys = new System.Collections.ArrayList();

		private string name = string.Empty;
		private string primaryKey = string.Empty;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="Table"/> class.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public Table()
		{
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="Table"/> class.
		/// </summary>
		/// <param name="name">The name of the table.</param>
		/////////////////////////////////////////////////////////////////////
		public Table(string name)
			: this()
		{
			Name = name;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets represents the columns.
		/// </summary>
		/// <value>
		/// Represents the columns.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public Hashtable Columns
		{
			get { return columns; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets represents the foreign keys.
		/// </summary>
		/// <value>
		/// Represents the foreign keys.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public ArrayList ForeignKeys
		{
			get { return foreignKeys; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets represents a table name.
		/// </summary>
		/// <value>
		/// Represents a table name.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets represents the primary key.
		/// </summary>
		/// <value>
		/// Represents the primary key.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public string PrimaryKey
		{
			get { return primaryKey; }
			set { primaryKey = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Writes out the table information.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <returns>DataTable.</returns>
		/////////////////////////////////////////////////////////////////////
		public static string Dump(DataTable table)
		{
			string output = string.Empty;

			if (null != table)
			{
				Log.Info(
					CultureInfo.InvariantCulture,
					m => m(StringTable.GetString(
						"TABLE",
						CultureInfo.InvariantCulture) + table.TableName));
				output = StringTable.GetString(
					"TABLE",
					CultureInfo.InvariantCulture) +
					table.TableName + Environment.NewLine;

				foreach (DataColumn column in table.Columns)
				{
					Log.Info(CultureInfo.InvariantCulture, m => m(
						StringTable.GetString(
							"TABDASH",
							CultureInfo.InvariantCulture) +
							column.ColumnName));
					output += StringTable.GetString(
						"TABDASH",
						CultureInfo.InvariantCulture) +
						column.ColumnName + Environment.NewLine;
				}

				Log.Info(CultureInfo.InvariantCulture, m => m(
					StringTable.GetString(
						"PRIMARYKEY",
						CultureInfo.InvariantCulture) + table.PrimaryKey));
				output += StringTable.GetString(
					"PRIMARYKEY",
					CultureInfo.InvariantCulture) +
					table.PrimaryKey + Environment.NewLine;
			}

			return output;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Add a column.
		/// </summary>
		/// <param name="column">The name of the column.</param>
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
		/// <returns>DataTable.</returns>
		/////////////////////////////////////////////////////////////////////
		public string Dump()
		{
			string output = StringTable.GetString(
				"TABLE",
				CultureInfo.InvariantCulture) + Name + Environment.NewLine;
			Log.Info(CultureInfo.InvariantCulture, m => m(output));

			foreach (DictionaryEntry column in Columns)
			{
				Log.Info(
					CultureInfo.InvariantCulture,
					m => m(StringTable.GetString(
						"TABDASH",
						CultureInfo.InvariantCulture) +
						((Column)column.Value).Name));
				output += StringTable.GetString(
					"TABDASH",
					CultureInfo.InvariantCulture) +
					((Column)column.Value).Name + Environment.NewLine;
			}

			Log.Info(
				CultureInfo.InvariantCulture,
				m => m(StringTable.GetString(
					"PRIMARYKEY",
					CultureInfo.InvariantCulture) + PrimaryKey));
			output += StringTable.GetString(
				"PRIMARYKEY",
				CultureInfo.InvariantCulture) + PrimaryKey +
				Environment.NewLine;

			foreach (ForeignKey foreignKey in ForeignKeys)
			{
				string format = string.Format(
					CultureInfo.InvariantCulture,
					"{0} {1} {2}",
					foreignKey.Name,
					foreignKey.ColumnName,
					foreignKey.ParentTable);

				Log.Info(
					CultureInfo.InvariantCulture,
					m => m(StringTable.GetString(
						"FOREIGNKEY",
						CultureInfo.InvariantCulture) + format));
				output += StringTable.GetString(
					"FOREIGNKEY",
					CultureInfo.InvariantCulture) + format +
					Environment.NewLine;
			}

			return output;
		}
	}
}
