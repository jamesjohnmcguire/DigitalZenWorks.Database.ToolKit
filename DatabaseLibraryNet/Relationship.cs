/////////////////////////////////////////////////////////////////////////////
// <copyright file="Relationship.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

using System;

/// <summary>
/// Holds schema relationship metadata from a referencing child table to a
/// referenced parent table.
/// </summary>
/// <remarks>Use ForeignKey for new code. Legacy Parent and Child properties
/// retain their historical reversed meanings.</remarks>
[Obsolete("Use ForeignKey instead.")]
public class Relationship
{
	/// <summary>
	/// Gets or sets name.
	/// </summary>
	/// <value>The name.</value>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the child table that contains the foreign key.
	/// </summary>
	public string ReferencingTable
	{
		get { return ParentTable; }
		set { ParentTable = value; }
	}

	/// <summary>
	/// Gets or sets the foreign key column in the referencing child table.
	/// </summary>
	public string ReferencingColumn
	{
		get { return ParentTableCol; }
		set { ParentTableCol = value; }
	}

	/// <summary>
	/// Gets or sets the parent table targeted by the foreign key.
	/// </summary>
	public string ReferencedTable
	{
		get { return ChildTable; }
		set { ChildTable = value; }
	}

	/// <summary>
	/// Gets or sets the target key column in the referenced parent table.
	/// </summary>
	public string ReferencedColumn
	{
		get { return ChildTableCol; }
		set { ChildTableCol = value; }
	}

	/// <summary>
	/// Gets or sets the referencing table using its legacy name.
	/// </summary>
	/// <value>The Parent Table.</value>
	public string ParentTable { get; set; }

	/// <summary>
	/// Gets or sets the referencing column using its legacy name.
	/// </summary>
	/// <value>The Parent Table Column.</value>
	public string ParentTableCol { get; set; }

	/// <summary>
	/// Gets or sets the referenced table using its legacy name.
	/// </summary>
	/// <value>The Child Table.</value>
	public string ChildTable { get; set; }

	/// <summary>
	/// Gets or sets the referenced column using its legacy name.
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
