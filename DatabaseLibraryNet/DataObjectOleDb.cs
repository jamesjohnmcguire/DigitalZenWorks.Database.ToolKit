/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataObjectOleDb.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

using System;
using System.Runtime.Versioning;

/// <summary>
/// OleDb base class for database collection classes.
/// </summary>
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public class DataObjectOleDb : DataObjectsBase
{
	/// <summary>
	/// Initializes a new instance of the <see cref="DataObjectOleDb"/>
	/// class.
	/// </summary>
	/// <param name="database">The DataStorageOleDb object to use.</param>
	public DataObjectOleDb(DataStorageOleDb database)
		: base(database)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DataObjectOleDb"/>
	/// class.
	/// </summary>
	/// <param name="databaseFilePath">The database file path.</param>
	public DataObjectOleDb(string databaseFilePath)
	{
		string connectionString =
			OleDbHelper.BuildConnectionString(databaseFilePath);

		Database = new DataStorageOleDb(connectionString);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="DataObjectOleDb"/> class
	/// using the historical base-constructor path.
	/// </summary>
	/// <param name="databaseType">The legacy database type.</param>
	/// <param name="databaseFilePath">The database file path.</param>
	/// <remarks>The base constructor retains its existing database-type
	/// restrictions. Use the path-only overload for new OLE DB callers.
	/// </remarks>
	[Obsolete("Use DataObjectOleDb(string databaseFilePath) instead.")]
	public DataObjectOleDb(
		DatabaseType databaseType, string databaseFilePath)
		: base(databaseType, databaseFilePath)
	{
		string connectionString =
			OleDbHelper.BuildConnectionString(databaseFilePath);

		Database = new DataStorageOleDb(connectionString);
	}
}
