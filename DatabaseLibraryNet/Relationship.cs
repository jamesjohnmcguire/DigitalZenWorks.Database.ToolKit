/////////////////////////////////////////////////////////////////////////////
// <copyright file="Relationship.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// A class to hold foreign key constraints temporarily.
	/// </summary>
	public class Relationship
	{
		private string child;
		private string childColumn;
		private string name;
		private ConstraintAction onDeleteAction;
		private ConstraintAction onUpdateAction;
		private string parent;
		private string parentColumn;

		/// <summary>
		/// Gets or sets name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets or sets parent Table.
		/// </summary>
		/// <value>The Parent Table.</value>
		public string ParentTable
		{
			get { return parent; }
			set { parent = value; }
		}

		/// <summary>
		/// Gets or sets parent Table Column.
		/// </summary>
		/// <value>The Parent Table Column.</value>
		public string ParentTableCol
		{
			get { return parentColumn; }
			set { parentColumn = value; }
		}

		/// <summary>
		/// Gets or sets child Table.
		/// </summary>
		/// <value>The Child Table.</value>
		public string ChildTable
		{
			get { return child; }
			set { child = value; }
		}

		/// <summary>
		/// Gets or sets child Table Column.
		/// </summary>
		/// <value>The Child Table Column.</value>
		public string ChildTableCol
		{
			get { return childColumn; }
			set { childColumn = value; }
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
	}
}
