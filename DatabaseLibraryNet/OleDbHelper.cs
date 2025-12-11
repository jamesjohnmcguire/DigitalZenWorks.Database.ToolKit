/////////////////////////////////////////////////////////////////////////////
// <copyright file="OleDbHelper.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Runtime.Versioning;
	using System.Text;

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
			string password = null,
			bool readOnly = false)
		{
			StringBuilder builder = new();
			builder.AppendFormat(
				CultureInfo.InvariantCulture, "Provider={0};", provider);
			builder.AppendFormat(
				CultureInfo.InvariantCulture, "Data Source={0};", databaseFile);

			if (!string.IsNullOrEmpty(password))
			{
				builder.AppendFormat(
					CultureInfo.InvariantCulture,
					"Jet OLEDB:Database Password={0};",
					password);
			}

			if (readOnly)
			{
				builder.Append("Mode=Read;");
			}

			string connectionString = builder.ToString();

			return connectionString;
		}

		/// <summary>
		/// Validates the Access database file.
		/// </summary>
		/// <param name="databaseFile">The database file.</param>
		/// <returns>Indicates whether the file is a valid access database file.
		/// </returns>
		public static bool ValidateAccessDatabaseFile(string databaseFile)
		{
			bool result = false;

			DatabaseType databaseType;

			bool exists = File.Exists(databaseFile);

			if (exists == true)
			{
				byte[] header
					= DataDefinition.GetDatabaseFileHeaderBytes(databaseFile);
				databaseType =
					DataDefinition.GetDatabaseTypeByFileHeaderBytes(header);

				// Fallback to extension-based detection
				if (databaseType == DatabaseType.Unknown)
				{
					databaseType =
						DataDefinition.GetDatabaseTypeByExtension(databaseFile);
				}

				if (databaseType != DatabaseType.OleDb)
				{
					const string message =
						"Database type is not supported for OleDb import.";
					throw new ArgumentException(
						message, nameof(databaseFile));
				}
				else
				{
					result = true;
				}
			}

			return result;
		}
	}
}
