//////////////////////////////////////////////////////////////////////////////
// <copyright file="ColumnPair.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
//////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit;

using System;

/// <summary>
/// A column identifier and the SQL value assigned to it in a statement.
/// </summary>
/// <remarks>
/// This type stores the supplied SQL text verbatim. Use identifiers from
/// trusted schema definitions and quote them as required by the SQL dialect.
/// Format raw text with <see cref="SqlWriter.GetTextLiteral"/> before passing
/// it as a value. Use the SQL keyword <c>NULL</c> for a database null.
/// </remarks>
public class ColumnPair
{
	/// <summary>
	/// Initializes a new instance of the <see cref="ColumnPair"/> class.
	/// </summary>
	/// <param name="columnName">
	/// The column identifier in the form required by the target SQL dialect.
	/// </param>
	/// <param name="sqlValue">
	/// The complete SQL value, such as a literal or trusted expression.
	/// </param>
	/// <exception cref="ArgumentNullException">
	/// A column identifier or SQL value is null.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// A column identifier or SQL value is empty or contains only whitespace.
	/// </exception>
	public ColumnPair(string columnName, string sqlValue)
	{
		ArgumentGuard.ThrowIfNull(columnName, nameof(columnName));

		if (string.IsNullOrWhiteSpace(columnName))
		{
			throw new ArgumentException(
				"A column name cannot be empty or whitespace.",
				nameof(columnName));
		}

		ArgumentGuard.ThrowIfNull(sqlValue, nameof(sqlValue));

		if (string.IsNullOrWhiteSpace(sqlValue))
		{
			throw new ArgumentException(
				"A SQL value cannot be empty or whitespace.",
				nameof(sqlValue));
		}

		ColumnName = columnName;
		SqlValue = sqlValue;
	}

	/// <summary>
	/// Gets the column identifier supplied to the constructor.
	/// </summary>
	public string ColumnName { get; }

	/// <summary>
	/// Gets the SQL value supplied to the constructor.
	/// </summary>
	public string SqlValue { get; }
}
