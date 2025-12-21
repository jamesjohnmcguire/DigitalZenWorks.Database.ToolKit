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
			this.Name = name;
			this.ChildColumn = columnName;
			this.ParentTable = parentTable;
			this.ParentColumn = parentTableColumn;
			this.OnDeleteAction = onDeleteAction;
			this.OnUpdateAction = onUpdateAction;
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
			this.Name = name;
			this.ChildTable = childTable;
			this.ChildColumn = childColumn;
			this.ParentTable = parentTable;
			this.ParentColumn = parentColumn;
			this.OnDeleteAction = onDeleteAction;
			this.OnUpdateAction = onUpdateAction;
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
		public ConstraintAction OnDeleteAction { get; set; }

		/// <summary>
		/// Gets or sets the action to take when a referenced row is updated in
		/// the parent table.
		/// </summary>
		/// <remarks>Use this property to specify how updates to parent table
		/// rows affect related rows in the child table. Common actions include
		/// cascading the update, setting related values to null, or restricting
		/// the update. The available actions are defined by the
		/// <see cref="ConstraintAction"/> enumeration.</remarks>
		public ConstraintAction OnUpdateAction { get; set; }

		/// <summary>
		/// Gets or sets the child column name.
		/// </summary>
		/// <value>
		/// Column Name.
		/// </value>
		public string ChildColumn { get; set; }

		/// <summary>
		/// Gets or sets the child table name.
		/// </summary>
		/// <value>
		/// Column Name.
		/// </value>
		public string ChildTable { get; set; }

		/// <summary>
		/// Gets or sets column Name.
		/// </summary>
		/// <value>
		/// Column Name.
		/// </value>
		public string ColumnName
		{
			get { return ChildColumn; }
			set { ChildColumn = value; }
		}

		/// <summary>
		/// Gets or sets foreign Key Name.
		/// </summary>
		/// <value>
		/// Foreign Key Name.
		/// </value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets parent Table Column.
		/// </summary>
		/// <value>
		/// Parent Table Column.
		/// </value>
		public string ParentColumn { get; set; }

		/// <summary>
		/// Gets or sets parent Table.
		/// </summary>
		/// <value>
		/// Parent Table.
		/// </value>
		public string ParentTable { get; set; }

		/// <summary>
		/// Gets or sets parent Table Column.
		/// </summary>
		/// <value>
		/// Parent Table Column.
		/// </value>
		public string ParentTableColumn
		{
			get { return ParentColumn; }
			set { ParentColumn = value; }
		}
	}
}
