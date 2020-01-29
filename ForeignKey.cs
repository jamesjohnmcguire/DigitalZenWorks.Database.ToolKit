/////////////////////////////////////////////////////////////////////////////
// <copyright file="ForeignKey.cs" company="James John McGuire">
// Copyright © 2006 - 2020 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Represents a foreign key
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
		/// Cascade On Delete
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public bool CascadeOnDelete
		{
			get { return cascadeOnDelete; }
			set { cascadeOnDelete = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Cascade On Update
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public bool CascadeOnUpdate
		{
			get { return cascadeOnUpdate; }
			set { cascadeOnUpdate = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Column Name
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public string ColumnName
		{
			get { return columnName; }
			set { columnName = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Foreign Key Name
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Parent Table
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public string ParentTable
		{
			get { return parentTable; }
			set { parentTable = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Parent Table Column.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public string ParentTableColumn
		{
			get { return parentTableColumn; }
			set { parentTableColumn = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Default Constructor
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
	}
}