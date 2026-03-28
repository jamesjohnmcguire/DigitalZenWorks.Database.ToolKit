/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseType.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

/// <summary>
/// Represents values to specify the types of databases.
/// </summary>
public enum DatabaseType
{
	/// <summary>
	/// Unknown.
	/// </summary>
	Unknown,

	/// <summary>
	/// OleDb.
	/// </summary>
	OleDb,

	/// <summary>
	/// Postgres.
	/// </summary>
	PostgresSql,

	/// <summary>
	/// SqlServer.
	/// </summary>
	SqlServer,

	/// <summary>
	/// Oracle.
	/// </summary>
	Oracle,

	/// <summary>
	/// MySql.
	/// </summary>
	MySql,

	/// <summary>
	/// SQLite.
	/// </summary>
	SQLite
}
