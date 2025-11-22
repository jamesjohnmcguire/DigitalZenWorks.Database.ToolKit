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
				string message = Strings.Table + table.TableName;
				Log.Info(message);

				output =
					Strings.Table + table.TableName + Environment.NewLine;

				foreach (DataColumn column in table.Columns)
				{
					message = Strings.TabDash + column.ColumnName;
					Log.Info(message);

					output += message + Environment.NewLine;
				}

				message = Strings.PrimaryKey + table.PrimaryKey;
				Log.Info(message);

				output += message + Environment.NewLine;
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
			string message;
			string output =
				Strings.Table + Name + Environment.NewLine;
			Log.Info(output);

			foreach (KeyValuePair<string, Column> column in Columns)
			{
				Column columnValue = column.Value;
				string name = columnValue.Name;

				message = Strings.TabDash + name;
				Log.Info(message);

				output += message + Environment.NewLine;
			}

			message = Strings.PrimaryKey + PrimaryKey;
			Log.Info(message);

			output += message + Environment.NewLine;

			foreach (ForeignKey foreignKey in ForeignKeys)
			{
				string format = string.Format(
					CultureInfo.InvariantCulture,
					"{0} {1} {2}",
					foreignKey.Name,
					foreignKey.ColumnName,
					foreignKey.ParentTable);

				message = Strings.ForeignKey + format;
				Log.Info(message);

				output += message + Environment.NewLine;
			}

			return output;
		}
	}
}
