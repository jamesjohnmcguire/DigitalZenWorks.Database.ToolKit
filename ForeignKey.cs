/////////////////////////////////////////////////////////////////////////////
// $Id: $
//
// Copyright (c) 2006-2014 by James John McGuire
// All rights reserved.
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
		/// Parent Table Column
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
		/// <param name="Name"></param>
		/// <param name="ColumnName"></param>
		/// <param name="ParentTable"></param>
		/// <param name="ParentTableColumn"></param>
		/// <param name="cascadeDelete"></param>
		/// <param name="cascadeUpdate"></param>
		/////////////////////////////////////////////////////////////////////
		public ForeignKey(
			string Name,
			string ColumnName,
			string ParentTable,
			string ParentTableColumn,
			bool cascadeDelete,
			bool cascadeUpdate)
		{
			name = Name;
			columnName = ColumnName;
			parentTable = ParentTable;
			parentTableColumn = ParentTableColumn;
			cascadeOnDelete = cascadeDelete;
			cascadeOnUpdate = cascadeUpdate;
		}
	}
}