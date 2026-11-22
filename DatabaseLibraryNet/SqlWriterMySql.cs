//////////////////////////////////////////////////////////////////////////////
// <copyright file="SqlWriterMySql.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
//////////////////////////////////////////////////////////////////////////////

#nullable enable

namespace DigitalZenWorks.Database.ToolKit;

using System;
using MySql.Data.MySqlClient;

/// <summary>
/// SQL writer with MySQL text literal and identifier formatting.
/// </summary>
/// <remarks>
/// Text literals require backslash escaping to be enabled on the target
/// server: SQL mode must not include NO_BACKSLASH_ESCAPES. Other inherited
/// operations retain the base writer's behavior.
/// </remarks>
public class SqlWriterMySql : SqlWriter
{
	/// <summary>
	/// Ensures a single MySQL identifier is enclosed in backticks.
	/// </summary>
	/// <param name="identifier">A raw name or a backtick-quoted name.</param>
	/// <returns>The backtick-quoted identifier.</returns>
	/// <remarks>
	/// An already quoted name is returned unchanged. Raw names containing a
	/// leading or trailing backtick must be supplied in quoted form. Quote
	/// each component of a qualified name separately. A dot in this input
	/// is part of the name.
	/// </remarks>
	/// <exception cref="ArgumentNullException">The name is null.</exception>
	/// <exception cref="ArgumentException">
	/// The name is blank, contains a null character, or has invalid quoting.
	/// </exception>
	public static string EnsureQuotedIdentifier(string identifier)
	{
		ArgumentGuard.ThrowIfNull(identifier, nameof(identifier));

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
		bool containsNull = identifier.Contains('\0', StringComparison.Ordinal);
#else
		bool containsNull =
			identifier.IndexOf("\0", StringComparison.Ordinal) >= 0;
#endif

		if (string.IsNullOrWhiteSpace(identifier) || containsNull == true)
		{
			throw new ArgumentException(
				"An identifier cannot be blank or contain a null character.",
				nameof(identifier));
		}

		string quotedIdentifier = identifier;
		int lastIndex = identifier.Length - 1;
		bool startsWithBacktick = identifier[0] == '`';
		bool endsWithBacktick = identifier[lastIndex] == '`';

		if (startsWithBacktick == true || endsWithBacktick == true)
		{
			if (!startsWithBacktick || !endsWithBacktick || lastIndex < 2)
			{
				throw new ArgumentException(
					"Supply one nonempty backtick-quoted identifier.",
					nameof(identifier));
			}

			for (int index = 1; index < lastIndex; index++)
			{
				if (identifier[index] == '`')
				{
					index++;
					if (index == lastIndex || identifier[index] != '`')
					{
						throw new ArgumentException(
							"Double backticks inside a quoted identifier.",
							nameof(identifier));
					}
				}
			}
		}
		else
		{
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
			string escaped = identifier.Replace(
				"`", "``", StringComparison.Ordinal);
#else
			string escaped = identifier.Replace("`", "``");
#endif
			quotedIdentifier = "`" + escaped + "`";
		}

		return quotedIdentifier;
	}

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
