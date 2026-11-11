/////////////////////////////////////////////////////////////////////////////
// <copyright file="OleDbHelper.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

using System;
using System.IO;
using System.Runtime.Versioning;
using DigitalZenWorks.Common.Utilities;

#nullable enable

/// <summary>
/// The OLE DB helper class.
/// </summary>
#if NET5_0_OR_GREATER
[SupportedOSPlatform("windows")]
#endif
public static class OleDbHelper
{
	/// <summary>
	/// Builds the connection string.
	/// </summary>
	/// <param name="databaseFile">The database file.</param>
	/// <param name="provider">The provider.</param>
	/// <param name="password">The password.</param>
	/// <param name="readOnly">Indicates if read only or not.</param>
	/// <returns>The connection string.</returns>
	public static string BuildConnectionString(
		string databaseFile,
		string provider = "Microsoft.ACE.OLEDB.12.0",
		string? password = null,
		bool readOnly = false)
	{
		string connectionString =
			$"Provider={provider};Data Source={databaseFile};";

		if (!string.IsNullOrEmpty(password))
		{
			connectionString += $"Jet OLEDB:Database Password={password};";
		}

		if (readOnly == true)
		{
			connectionString += "Mode=Read;";
		}

		return connectionString;
	}

	/// Method <c>CreateAccessDatabaseFile.</c>
	/// <summary>
	/// Creates an empty MDB (MS Jet / Access database) file.
	/// </summary>
	/// <param name="filePath">The file path of the database.</param>
	/// <returns>A values indicating success or not.</returns>
	public static bool CreateAccessDatabaseFile(string filePath)
	{
		return FileUtils.CreateFileFromEmbeddedResource(
			"DigitalZenWorks.Database.ToolKit.Template.accdb",
			filePath);
	}

	/// Method <c>ExportToCsv.</c>
	/// <summary>
	/// Export all tables to similarly named csv files.
	/// </summary>
	/// <param name="databaseFile">The database file to use.</param>
	/// <param name="csvPath">The csv file to export to.</param>
	/// <returns>A values indicating success or not.</returns>
	public static bool ExportToCsv(
		string databaseFile, string csvPath)
	{
		bool returnCode = false;

		DatabaseType databaseType =
			DatabaseFileInspector.GetDatabaseFileType(databaseFile);
		string connectionString =
			DataStorage.GetConnectionString(databaseType, databaseFile);

		using DataStorageOleDb database = new(connectionString);

		returnCode =
			DatabaseUtilities.ExportDatabaseToCsv(database, csvPath);

		database.Shutdown();

		return returnCode;
	}

	/// <summary>
	/// Checks whether an existing file is identified as an Access database.
	/// </summary>
	/// <remarks>Uses header detection with extension fallback without opening
	/// a database connection.</remarks>
	/// <param name="databaseFile">The database file.</param>
	/// <returns>True for an Access file; false if the file does not exist.
	/// </returns>
	/// <exception cref="ArgumentException">The existing file is not
	/// identified as an Access database.</exception>
	/// <exception cref="IOException">The file header cannot be read.
	/// </exception>
	public static bool ValidateAccessDatabaseFile(string databaseFile)
	{
		bool result = false;

		bool exists = File.Exists(databaseFile);

		if (exists == true)
		{
			DatabaseType databaseType =
				DatabaseFileInspector.GetDatabaseFileType(databaseFile);

			if (databaseType != DatabaseType.OleDb)
			{
				const string message =
					"Database type is not supported for OleDb import.";
				throw new ArgumentException(message, nameof(databaseFile));
			}
			else
			{
				result = true;
			}
		}

		return result;
	}
}
