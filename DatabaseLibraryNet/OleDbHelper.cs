/////////////////////////////////////////////////////////////////////////////
// <copyright file="OleDbHelper.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.Globalization;
	using System.Runtime.Versioning;
	using System.Text;
	using global::Common.Logging;

	/// <summary>
	/// The OLE DB helper class.
	/// </summary>
#if NET5_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public static class OleDbHelper
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
			StringBuilder builder = new StringBuilder();
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
		/// Executes the queries.
		/// </summary>
		/// <param name="database">The database object.</param>
		/// <param name="queries">The list of queries.</param>
		/// <returns>True if successful, false otherwise.</returns>
		public static bool ExecuteQueries(
			DataStorage database, IReadOnlyList<string> queries)
		{
			bool result = false;

			ArgumentNullException.ThrowIfNull(database);

			if (queries != null)
			{
				foreach (string sqlQuery in queries)
				{
					try
					{
						string message = Strings.Command + sqlQuery;
						Log.Info(message);

						database.ExecuteNonQuery(sqlQuery);
					}
					catch (Exception exception) when
						(exception is ArgumentNullException ||
						exception is OutOfMemoryException ||
						exception is System.Data.OleDb.OleDbException)
					{
						string message = Strings.Exception + exception;
						Log.Error(message);
					}
					catch (Exception exception)
					{
						string message = Strings.Exception + exception;
						Log.Error(message);

						throw;
					}
				}

				result = true;
			}

			return result;
		}
	}
}
