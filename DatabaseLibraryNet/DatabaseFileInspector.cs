/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseFileInspector.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

using System;
using System.IO;
using global::Common.Logging;

/// <summary>
/// Identifies database file formats using file headers and extensions.
/// </summary>
public static class DatabaseFileInspector
{
	/// <summary>
	/// Diagnostics object.
	/// </summary>
	private static readonly ILog Log = LogManager.GetLogger(
		System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	/// <summary>
	/// Get database file header bytes.
	/// </summary>
	/// <param name="databaseFile">The database file.</param>
	/// <returns>The database header bytes.</returns>
	public static byte[] GetDatabaseFileHeaderBytes(string databaseFile)
	{
		byte[] header = null;

		try
		{
			using FileStream stream = new(
				databaseFile,
				FileMode.Open,
				FileAccess.Read,
				FileShare.ReadWrite);

			// Read enough bytes to check all signatures
			header = new byte[32];
			int bytesRead = stream.Read(header, 0, header.Length);
		}
		catch (Exception exception) when
			(exception is ArgumentNullException)
		{
			Log.Error(Strings.Exception + exception);
		}

		return header;
	}

	/// <summary>
	/// Gets the database file type using its header and extension.
	/// </summary>
	/// <param name="databaseFile">The database file.</param>
	/// <returns>The database type.</returns>
	public static DatabaseType GetDatabaseFileType(string databaseFile)
	{
		DatabaseType databaseType = DatabaseType.Unknown;

		bool exists = File.Exists(databaseFile);

		if (exists == true)
		{
			byte[] header = GetDatabaseFileHeaderBytes(databaseFile);
			databaseType = GetDatabaseTypeByFileHeaderBytes(header);

			// Fallback to extension-based detection
			if (databaseType == DatabaseType.Unknown)
			{
				databaseType = GetDatabaseTypeByExtension(databaseFile);
			}
		}

		return databaseType;
	}

	/// <summary>
	/// Get database type by file extension.
	/// </summary>
	/// <param name="databaseFile">The database file.</param>
	/// <returns>The database type.</returns>
	public static DatabaseType GetDatabaseTypeByExtension(
		string databaseFile)
	{
		string extension = Path.GetExtension(databaseFile);
#pragma warning disable CA1308
		extension = extension.ToLowerInvariant();
#pragma warning restore CA1308

		DatabaseType databaseType = extension switch
		{
			".db" or ".sqlite" or ".sqlite3" or ".db3" or ".sdb" =>
				DatabaseType.SQLite,
			".mdb" or ".accdb" => DatabaseType.OleDb,
			".mdf" => DatabaseType.SqlServer,
			_ => DatabaseType.Unknown
		};

		return databaseType;
	}

	/// <summary>
	/// Get database type by file header bytes.
	/// </summary>
	/// <param name="header">The header bytes.</param>
	/// <returns>The database type.</returns>
	public static DatabaseType GetDatabaseTypeByFileHeaderBytes(
		byte[] header)
	{
		DatabaseType databaseType = DatabaseType.Unknown;

		if (header == null || header.Length < 4)
		{
			databaseType = DatabaseType.Unknown;
		}
		else if (header.Length >= 16 &&
			header[0] == 0x53 && header[1] == 0x51 &&
			header[2] == 0x4C && header[3] == 0x69 &&
			header[4] == 0x74 && header[5] == 0x65 &&
			header[6] == 0x20 && header[7] == 0x66 &&
			header[8] == 0x6F && header[9] == 0x72 &&
			header[10] == 0x6D && header[11] == 0x61 &&
			header[12] == 0x74 && header[13] == 0x20 &&
			header[14] == 0x33)
		{
			// SQLite: "SQLite format 3\0" (starts at byte 0)
			databaseType = DatabaseType.SQLite;
		}
		else if (header.Length >= 20 &&
			header[0] == 0x00 && header[1] == 0x01 &&
			header[2] == 0x00 && header[3] == 0x00 &&
			header[4] == 0x53 && header[5] == 0x74 &&
			header[6] == 0x61 && header[7] == 0x6E &&
			header[8] == 0x64 && header[9] == 0x61 &&
			header[10] == 0x72 && header[11] == 0x64 &&
			header[12] == 0x20 && header[13] == 0x41 &&
			header[14] == 0x43 && header[15] == 0x45)
		{
			// MS Access 2007+ (.accdb): 0x00, 0x01, 0x00, 0x00,
			// "Standard ACE DB"
			databaseType = DatabaseType.OleDb;
		}
		else if (header.Length >= 20 &&
			header[0] == 0x00 && header[1] == 0x01 &&
			header[2] == 0x00 && header[3] == 0x00 &&
			header[4] == 0x53 && header[5] == 0x74 &&
			header[6] == 0x61 && header[7] == 0x6E &&
			header[8] == 0x64 && header[9] == 0x61 &&
			header[10] == 0x72 && header[11] == 0x64 &&
			header[12] == 0x20 && header[13] == 0x4A &&
			header[14] == 0x65 && header[15] == 0x74)
		{
			// MS Access 97-2003 (.mdb): 0x00, 0x01, 0x00, 0x00,
			// "Standard Jet DB"
			databaseType = DatabaseType.OleDb;
		}
		else if (header[0] == 0x01 && header[1] == 0x0F &&
			header[2] == 0x00 && header[3] == 0x00)
		{
			// SQL Server .mdf: 0x01, 0x0F, 0x00, 0x00
			// (page header signature)
			databaseType = DatabaseType.SqlServer;
		}

		// Firebird/Interbase: Check for specific page size markers
		// This is trickier and might need extension fallback
		return databaseType;
	}
}
