/////////////////////////////////////////////////////////////////////////////
// <copyright file="ForeignKey.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

using System;

/// <summary>
/// Represents a foreign key from a referencing table to a referenced table.
/// </summary>
/// <remarks>
/// The referencing table is conventionally the child; the referenced table
/// is the parent. Use the explicit Referencing and Referenced properties.
/// Legacy Parent and Child properties retain their historical assignments.
/// </remarks>
public class ForeignKey
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ForeignKey"/> class.
	/// </summary>
	/// <remarks>This legacy overload leaves ChildTable unset and assigns
	/// the local column to ChildColumn. It does not produce the complete
	/// representation expected by the schema writers. Assignments are
	/// preserved for compatibility; use Create for new code.</remarks>
	/// <param name="name">The name of the foreign key.</param>
	/// <param name="columnName">The column name.</param>
	/// <param name="parentTable">The parent table.</param>
	/// <param name="parentTableColumn">The parent table column.</param>
	/// <param name="onDeleteAction">The on delete action.</param>
	/// <param name="onUpdateAction">The on update action.</param>
	[Obsolete("Use ForeignKey.Create instead.")]
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
	/// <remarks>Parameter names preserve the legacy reversed terminology.
	/// Use Create for explicit conventional relationship names.</remarks>
	/// <param name="parentTable">The referencing table (legacy name).</param>
	/// <param name="parentColumn">The referencing column (legacy name).</param>
	/// <param name="childTable">The referenced table (legacy name).</param>
	/// <param name="childColumn">The referenced column (legacy name).</param>
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
		get { return ParentColumn; }
		set { ParentColumn = value; }
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
		get { return ChildColumn; }
		set { ChildColumn = value; }
	}

	/// <summary>
	/// Gets or sets the referenced column using its legacy name.
	/// </summary>
	/// <value>
	/// The child table column name.
	/// </value>
	public string ChildColumn { get; set; }

	/// <summary>
	/// Gets or sets the referenced table using its legacy name.
	/// </summary>
	/// <value>
	/// The child table name.
	/// </value>
	public string ChildTable { get; set; }

	/// <summary>
	/// Gets or sets the legacy alias for ChildColumn (the referenced column).
	/// </summary>
	/// <value>
	/// The column name.
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
	/// The foreign key name.
	/// </value>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the referencing column using its legacy name.
	/// </summary>
	/// <value>
	/// The parent table column name.
	/// </value>
	public string ParentColumn { get; set; }

	/// <summary>
	/// Gets or sets the referencing table using its legacy name.
	/// </summary>
	/// <value>
	/// The parent table name.
	/// </value>
	public string ParentTable { get; set; }

	/// <summary>
	/// Gets or sets the legacy alias for the referencing column.
	/// </summary>
	/// <value>
	/// The parent table column name.
	/// </value>
	public string ParentTableColumn
	{
		get { return ParentColumn; }
		set { ParentColumn = value; }
	}

	/// <summary>
	/// Creates a complete foreign key using conventional relationship roles.
	/// </summary>
	/// <param name="name">The constraint name.</param>
	/// <param name="referencedTable">The referenced parent table.</param>
	/// <param name="referencedColumn">The referenced parent key column.</param>
	/// <param name="referencingTable">The child table containing the key.
	/// </param>
	/// <param name="referencingColumn">The child foreign key column.</param>
	/// <param name="onDeleteAction">The action on parent deletion.</param>
	/// <param name="onUpdateAction">The action on parent key update.</param>
	/// <returns>A complete foreign key for a single column pair.</returns>
	/// <exception cref="ArgumentNullException">An identifier is null.
	/// </exception>
	/// <exception cref="ArgumentException">An identifier is empty or consists
	/// only of whitespace.</exception>
	/// <remarks>Validation applies at creation. Setters remain permissive for
	/// compatibility with existing callers; this is not an immutable model.
	/// The factory requires a name even though legacy constructors do not.
	/// </remarks>
	public static ForeignKey Create(
		string name,
		string referencedTable,
		string referencedColumn,
		string referencingTable,
		string referencingColumn,
		ConstraintAction onDeleteAction,
		ConstraintAction onUpdateAction)
	{
		ValidateIdentifier(name, nameof(name));
		ValidateIdentifier(referencedTable, nameof(referencedTable));
		ValidateIdentifier(referencedColumn, nameof(referencedColumn));
		ValidateIdentifier(referencingTable, nameof(referencingTable));
		ValidateIdentifier(referencingColumn, nameof(referencingColumn));

		return new ForeignKey(
			name,
			referencingTable,
			referencingColumn,
			referencedTable,
			referencedColumn,
			onDeleteAction,
			onUpdateAction);
	}

	private static void ValidateIdentifier(string value, string parameterName)
	{
		if (value == null)
		{
			throw new ArgumentNullException(parameterName);
		}

		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException(
				"An identifier cannot be empty or whitespace.", parameterName);
		}
	}
}
