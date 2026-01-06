/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataObjectOleDb.cs" company="James John McGuire">
// Copyright © 2006 - 2026 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
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
		/// <param name="databaseType">The database type.</param>
		/// <param name="databaseFilePath">The database file path.</param>
		public DataObjectOleDb(
			DatabaseType databaseType, string databaseFilePath)
			: base(databaseType, databaseFilePath)
		{
			string connectionString =
				OleDbHelper.BuildConnectionString(databaseFilePath);

			Database = new DataStorageOleDb(connectionString);
		}
	}
}
