/////////////////////////////////////////////////////////////////////////////
// <copyright file="ForeignKey.cs" company="James John McGuire">
// Copyright © 2006 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Represents a foreign key.
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class ForeignKey
	{
		private bool cascadeOnDelete;
		private bool cascadeOnUpdate;
		private string columnName;
		private string name;
		private string parentTable;
		private string parentTableColumn;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="ForeignKey"/> class.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="columnName"></param>
		/// <param name="parentTable"></param>
		/// <param name="parentTableColumn"></param>
		/// <param name="cascadeDelete"></param>
		/// <param name="cascadeUpdate"></param>
		/////////////////////////////////////////////////////////////////////
		public ForeignKey(
			string name,
			string columnName,
			string parentTable,
			string parentTableColumn,
			bool cascadeDelete,
			bool cascadeUpdate)
		{
			this.name = name;
			this.columnName = columnName;
			this.parentTable = parentTable;
			this.parentTableColumn = parentTableColumn;
			this.cascadeOnDelete = cascadeDelete;
			this.cascadeOnUpdate = cascadeUpdate;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets a value indicating whether cascade On Delete.
		/// </summary>
		/// <value>
		/// A value indicating whether cascade On Delete.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public bool CascadeOnDelete
		{
			get { return cascadeOnDelete; }
			set { cascadeOnDelete = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets a value indicating whether cascade On Update.
		/// </summary>
		/// <value>
		/// A value indicating whether cascade On Update.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public bool CascadeOnUpdate
		{
			get { return cascadeOnUpdate; }
			set { cascadeOnUpdate = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets column Name.
		/// </summary>
		/// <value>
		/// Column Name.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public string ColumnName
		{
			get { return columnName; }
			set { columnName = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets foreign Key Name.
		/// </summary>
		/// <value>
		/// Foreign Key Name.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets parent Table.
		/// </summary>
		/// <value>
		/// Parent Table.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public string ParentTable
		{
			get { return parentTable; }
			set { parentTable = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets parent Table Column.
		/// </summary>
		/// <value>
		/// Parent Table Column.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public string ParentTableColumn
		{
			get { return parentTableColumn; }
			set { parentTableColumn = value; }
		}
	}
}