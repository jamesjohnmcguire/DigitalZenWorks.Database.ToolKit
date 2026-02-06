/////////////////////////////////////////////////////////////////////////////
// <copyright file="ConstraintAction.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

/// <summary>
/// Specifies the action to take when a constraint is violated in a
/// relational data operation.
/// </summary>
/// <remarks>Use this enumeration to define how related data should be
/// handled when a constraint, such as a foreign key relationship, is
/// affected. The available actions include performing no operation,
/// cascading the change, or setting related values to null. The specific
/// behavior depends on the context in which the constraint is applied.
/// </remarks>
public enum ConstraintAction
{
	/// <summary>
	/// Indicates that no action will be performed.
	/// </summary>
	NoAction,

	/// <summary>
	/// Gets or sets a value indicating whether changes to this entity are
	/// automatically propagated to related entities.
	/// </summary>
	/// <remarks>When enabled, operations such as delete or update will
	/// cascade to associated entities according to the configured
	/// relationship rules. This property is commonly used in
	/// object-relational mapping scenarios to control referential
	/// integrity.</remarks>
	Cascade,

	/// <summary>
	/// Gets or sets a value indicating whether the field should be set to
	/// null in the database.
	/// </summary>
	SetNull
}
