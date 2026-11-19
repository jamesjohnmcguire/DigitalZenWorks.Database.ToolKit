/////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterMySql.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit;

using MySql.Data.MySqlClient;

/// <summary>
/// SQL writer with MySQL text-literal formatting.
/// </summary>
/// <remarks>
/// Text literals require backslash escaping to be enabled on the target
/// server: SQL mode must not include NO_BACKSLASH_ESCAPES. Other inherited
/// operations retain the base writer's behavior.
/// </remarks>
public class SqlWriterMySql : SqlWriter
{
	/// <inheritdoc />
	public override string GetTextLiteral(string? value)
	{
		string escapedValue = "NULL";

		if (value != null)
		{
			escapedValue = MySqlHelper.EscapeString(value);
			escapedValue = "'" + escapedValue + "'";
		}

		return escapedValue;
	}
}
