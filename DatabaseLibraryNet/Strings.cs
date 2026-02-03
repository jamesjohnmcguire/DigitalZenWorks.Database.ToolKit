/////////////////////////////////////////////////////////////////////////////
// <copyright file="Strings.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// Provides constant string values used for formatting and labeling
	/// within the application.
	/// </summary>
	/// <remarks>This class contains string constants commonly used for
	/// constructing messages, labels, and log entries related to database
	/// operations and error reporting. The constants are intended for internal
	/// use and help ensure consistency in output formatting throughout
	/// the application.</remarks>
	internal sealed class Strings
	{
		/// <summary>
		/// Represents the string value " begin" used to indicate the start
		/// of an operation or state.
		/// </summary>
		public const string Begin = " begin";

		/// <summary>
		/// Represents the prefix used to identify command messages in the
		/// protocol or log output.
		/// </summary>
		public const string Command = "Command: ";

		/// <summary>
		/// Represents the standard prefix used for exception messages.
		/// </summary>
		public const string Exception = "Exception: ";

		/// <summary>
		/// Represents the prefix used to identify foreign key constraints in
		/// serialized or formatted output.
		/// </summary>
		public const string ForeignKey = "\t-FK:";

		/// <summary>
		/// Represents the string identifier used to denote a primary key in
		/// a database schema definition.
		/// </summary>
		public const string PrimaryKey = "\tPK =";

		/// <summary>
		/// Represents a tab character followed by a dash ("\t-").
		/// </summary>
		public const string TabDash = "\t-";

		/// <summary>
		/// Represents the prefix used to identify table-related entries in a
		/// formatted string.
		/// </summary>
		public const string Table = "Table: ";

		/// <summary>
		/// Represents the warning message indicating that a column has been
		/// set to the 'Other' type.
		/// </summary>
		public const string WarningOther =
			"This column has been set to the 'Other' type: ";
	}
}

