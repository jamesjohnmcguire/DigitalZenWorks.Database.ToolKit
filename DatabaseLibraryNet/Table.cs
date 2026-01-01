/////////////////////////////////////////////////////////////////////////////
// <copyright file="Table.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Data;
	using System.Text;
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
		/// Gets the columns.
		/// </summary>
		/// <value>
		/// The columns.
		/// </value>
		public Dictionary<string, Column> Columns { get; } = [];

		/// <summary>
		/// Gets the foreign keys.
		/// </summary>
		/// <value>
		/// The foreign keys.
		/// </value>
		public Collection<ForeignKey> ForeignKeys { get; } = [];

		/// <summary>
		/// Gets or sets the table name.
		/// </summary>
		/// <value>
		/// The table name.
		/// </value>
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Gets or sets the primary key.
		/// </summary>
		/// <value>
		/// The primary key.
		/// </value>
		public string PrimaryKey { get; set; }

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
				Dictionary<string, Column> columns = [];

				foreach (DataColumn dataColumn in table.Columns)
				{
					Column column = new();
					column.Name = dataColumn.ColumnName;

					foreach (DataColumn primaryKeyColumn in table.PrimaryKey)
					{
						if (dataColumn.ColumnName ==
							primaryKeyColumn.ColumnName)
						{
							column.Primary = true;
						}
					}

					columns.Add(dataColumn.ColumnName, column);
				}

				output = Dump(table.TableName, columns);
			}

			return output;
		}

		/// <summary>
		/// Writes out the table information.
		/// </summary>
		/// <param name="tableName">The name of the table.</param>
		/// <param name="columns">The columns of the table.</param>
		/// <returns>A text of table information.</returns>
		public static string Dump(
			string tableName, Dictionary<string, Column> columns)
		{
			StringBuilder output = new();
			string message = $"{Strings.Table}{tableName}";
			output.AppendLine(message);
			Log.Info(message);

			string primaryKey = string.Empty;

			if (columns != null)
			{
				foreach (KeyValuePair<string, Column> column in columns)
				{
					Column columnValue = column.Value;

					message = $"{Strings.TabDash}{columnValue.Name}";
					output.AppendLine(message);
					Log.Info(message);

					if (columnValue.Primary == true)
					{
						primaryKey = columnValue.Name;
					}
				}
			}

			message = $"{Strings.PrimaryKey}{primaryKey}";
			output.AppendLine(message);
			Log.Info(message);

			string result = output.ToString();
			return result;
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
		/// <returns>A text of table information.</returns>
		public string Dump()
		{
			string output = Dump(Name, Columns);

			return output;
		}
	}
}

