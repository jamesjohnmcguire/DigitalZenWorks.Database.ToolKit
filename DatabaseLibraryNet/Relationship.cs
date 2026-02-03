/////////////////////////////////////////////////////////////////////////////
// <copyright file="Relationship.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// A class to hold foreign key constraints temporarily.
	/// </summary>
	public class Relationship
	{
		/// <summary>
		/// Gets or sets name.
		/// </summary>
		/// <value>The name.</value>
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets parent Table.
		/// </summary>
		/// <value>The Parent Table.</value>
		public string ParentTable { get; set; }

		/// <summary>
		/// Gets or sets parent Table Column.
		/// </summary>
		/// <value>The Parent Table Column.</value>
		public string ParentTableCol { get; set; }

		/// <summary>
		/// Gets or sets child Table.
		/// </summary>
		/// <value>The Child Table.</value>
		public string ChildTable { get; set; }

		/// <summary>
		/// Gets or sets child Table Column.
		/// </summary>
		/// <value>The Child Table Column.</value>
		public string ChildTableCol { get; set; }

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
		/// Gets or sets the action to perform when a related row is deleted in
		/// the parent table.
		/// </summary>
		/// <remarks>Use this property to specify how deletions in the parent
		/// table affect related rows in the child table. Common actions include
		/// cascading the delete, setting related values to null, or restricting
		/// the delete operation. The available actions are defined by the
		/// <see cref="ConstraintAction"/> enumeration.</remarks>
		public ConstraintAction OnDeleteAction { get; set; }
	}
}

