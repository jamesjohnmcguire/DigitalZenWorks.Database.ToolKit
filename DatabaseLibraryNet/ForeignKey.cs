/////////////////////////////////////////////////////////////////////////////
// <copyright file="ForeignKey.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// Represents a foreign key.
	/// </summary>
	public class ForeignKey
	{
		private string childColumn;
		private string childTable;
		private string name;
		private ConstraintAction onDeleteAction;
		private ConstraintAction onUpdateAction;
		private string parentTable;
		private string parentColumn;

		/// <summary>
		/// Initializes a new instance of the <see cref="ForeignKey"/> class.
		/// </summary>
		/// <param name="name">The name of the foreign key.</param>
		/// <param name="columnName">The column name.</param>
		/// <param name="parentTable">The parent table.</param>
		/// <param name="parentTableColumn">The parent table column.</param>
		/// <param name="onDeleteAction">The on delete action.</param>
		/// <param name="onUpdateAction">The on update action.</param>
		public ForeignKey(
			string name,
			string columnName,
			string parentTable,
			string parentTableColumn,
			ConstraintAction onDeleteAction,
			ConstraintAction onUpdateAction)
		{
			this.name = name;
			this.childColumn = columnName;
			this.parentTable = parentTable;
			this.parentColumn = parentTableColumn;
			this.onDeleteAction = onDeleteAction;
			this.onUpdateAction = onUpdateAction;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ForeignKey"/> class.
		/// </summary>
		/// <param name="name">The name of the foreign key.</param>
		/// <param name="parentTable">The parent table.</param>
		/// <param name="parentColumn">The parent table column.</param>
		/// <param name="childTable">The child table.</param>
		/// <param name="childColumn">The child column name.</param>
		/// <param name="onDeleteAction">The on delete action.</param>
		/// <param name="onUpdateAction">The on update action.</param>
		public ForeignKey(
			string name,
			string parentTable,
			string parentColumn,
			string childTable,
			string childColumn,
			ConstraintAction onDeleteAction,
			ConstraintAction onUpdateAction)
		{
			this.name = name;
			this.childTable = childTable;
			this.childColumn = childColumn;
			this.parentTable = parentTable;
			this.parentColumn = parentColumn;
			this.onDeleteAction = onDeleteAction;
			this.onUpdateAction = onUpdateAction;
		}

		/// <summary>
		/// Gets or sets the action to perform when a related row is deleted in
		/// the parent table.
		/// </summary>
		/// <remarks>Use this property to specify how deletions in the parent
		/// table affect related rows in the child table. Common actions include
		/// cascading the delete, setting related values to null, or restricting
		/// the delete operation. The available actions are defined by the
		/// <see cref="ConstraintAction"/> enumeration.</remarks>
		public ConstraintAction OnDeleteAction
		{
			get { return onDeleteAction; }
			set { onDeleteAction = value; }
		}

		/// <summary>
		/// Gets or sets the action to take when a referenced row is updated in
		/// the parent table.
		/// </summary>
		/// <remarks>Use this property to specify how updates to parent table
		/// rows affect related rows in the child table. Common actions include
		/// cascading the update, setting related values to null, or restricting
		/// the update. The available actions are defined by the
		/// <see cref="ConstraintAction"/> enumeration.</remarks>
		public ConstraintAction OnUpdateAction
		{
			get { return onUpdateAction; }
			set { onUpdateAction = value; }
		}

		/// <summary>
		/// Gets or sets the child column name.
		/// </summary>
		/// <value>
		/// Column Name.
		/// </value>
		public string ChildColumn
		{
			get { return childColumn; }
			set { childColumn = value; }
		}

		/// <summary>
		/// Gets or sets the child table name.
		/// </summary>
		/// <value>
		/// Column Name.
		/// </value>
		public string ChildTable
		{
			get { return childTable; }
			set { childTable = value; }
		}

		/// <summary>
		/// Gets or sets column Name.
		/// </summary>
		/// <value>
		/// Column Name.
		/// </value>
		public string ColumnName
		{
			get { return childColumn; }
			set { childColumn = value; }
		}

		/// <summary>
		/// Gets or sets foreign Key Name.
		/// </summary>
		/// <value>
		/// Foreign Key Name.
		/// </value>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets or sets parent Table Column.
		/// </summary>
		/// <value>
		/// Parent Table Column.
		/// </value>
		public string ParentColumn
		{
			get { return parentColumn; }
			set { parentColumn = value; }
		}

		/// <summary>
		/// Gets or sets parent Table.
		/// </summary>
		/// <value>
		/// Parent Table.
		/// </value>
		public string ParentTable
		{
			get { return parentTable; }
			set { parentTable = value; }
		}

		/// <summary>
		/// Gets or sets parent Table Column.
		/// </summary>
		/// <value>
		/// Parent Table Column.
		/// </value>
		public string ParentTableColumn
		{
			get { return parentColumn; }
			set { parentColumn = value; }
		}
	}
}
