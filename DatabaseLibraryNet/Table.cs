/////////////////////////////////////////////////////////////////////////////
// <copyright file="Table.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Globalization;
	using System.Reflection;
	using System.Resources;
	using global::Common.Logging;

	/// <summary>
	/// Represents a database table.
	/// </summary>
	public class Table
	{
		/// <summary>
		/// Diagnostics object.
		/// </summary>
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static readonly ResourceManager StringTable = new (
			"DigitalZenWorks.Database.ToolKit.Resources",
			Assembly.GetExecutingAssembly());

		private readonly Dictionary<string, Column> columns = [];
		private readonly Collection<ForeignKey> foreignKeys = [];

		private string name = string.Empty;
		private string primaryKey = string.Empty;

		/// <summary>
		/// Initializes a new instance of the <see cref="Table"/> class.
		/// </summary>
		public Table()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Table"/> class.
		/// </summary>
		/// <param name="name">The name of the table.</param>
		public Table(string name)
			: this()
		{
			Name = name;
		}

		/// <summary>
		/// Gets represents the columns.
		/// </summary>
		/// <value>
		/// Represents the columns.
		/// </value>
		public Dictionary<string, Column> Columns
		{
			get { return columns; }
		}

		/// <summary>
		/// Gets represents the foreign keys.
		/// </summary>
		/// <value>
		/// Represents the foreign keys.
		/// </value>
		public Collection<ForeignKey> ForeignKeys
		{
			get { return foreignKeys; }
		}

		/// <summary>
		/// Gets or sets represents a table name.
		/// </summary>
		/// <value>
		/// Represents a table name.
		/// </value>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets or sets represents the primary key.
		/// </summary>
		/// <value>
		/// Represents the primary key.
		/// </value>
		public string PrimaryKey
		{
			get { return primaryKey; }
			set { primaryKey = value; }
		}

		/// <summary>
		/// Writes out the table information.
		/// </summary>
		/// <param name="table">The name of the table.</param>
		/// <returns>DataTable.</returns>
		public static string Dump(DataTable table)
		{
			string output = string.Empty;

			if (table != null)
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

		/// <summary>
		/// Add a column.
		/// </summary>
		/// <param name="column">The name of the column.</param>
		public void AddColumn(Column column)
		{
			if (column != null)
			{
				Columns.Add(column.Name, column);
			}
		}

		/// <summary>
		/// Writes out the table information.
		/// </summary>
		/// <returns>DataTable.</returns>
		public string Dump()
		{
			string output = StringTable.GetString(
				"TABLE",
				CultureInfo.InvariantCulture) + Name + Environment.NewLine;
			Log.Info(CultureInfo.InvariantCulture, m => m(output));

			foreach (KeyValuePair<string, Column> column in Columns)
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
