/////////////////////////////////////////////////////////////////////////////
// <copyright file="DataDefinitionOleDb.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Runtime.Versioning;
	using global::Common.Logging;

	/// Class <c>DataDefinitionOleDb.</c>
	/// <summary>
	/// Class for OleDb version support on operations on complete data
	/// storage containers.
	/// </summary>
#if NET5_0_OR_GREATER
	[SupportedOSPlatform("windows")]
#endif
	public static class DataDefinitionOleDb
	{
		private static readonly ILog Log = LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// Method <c>ExportSchema.</c>
		/// <summary>
		/// Export all tables to similarly named csv files.
		/// </summary>
		/// <returns>A values indicating success or not.</returns>
		/// <param name="databaseFile">The database file to use.</param>
		/// <param name="schemaFile">The schema file to export to.</param>
		public static bool ExportSchema(
			string databaseFile, string schemaFile)
		{
			bool successCode = false;

			try
			{
				using OleDbSchema schema = new(databaseFile);
				Collection<Table> tables = schema.GetSchema();

				tables = DataStoreStructure.OrderTables(tables);

				SqlWriterOleDb sqlWriter = new();
				string schemaText = sqlWriter.GetTablesCreateStatements(tables);

				File.WriteAllText(schemaFile, schemaText);

				successCode = true;
			}
			catch (Exception exception) when
				(exception is ArgumentNullException ||
				exception is ArgumentException ||
				exception is InvalidOperationException)
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

			return successCode;
		}

		/// <summary>
		/// Retrieves the schema information for all tables in the specified
		/// database file.
		/// </summary>
		/// <param name="databaseFile">The path to the database file from
		/// which to retrieve schema information. Must refer to a valid and
		/// accessible database file.</param>
		/// <returns>A collection of <see cref="Table"/> objects representing
		/// the tables defined in the database. The collection will be empty
		/// if no tables are found.</returns>
		public static Collection<Table> GetSchema(string databaseFile)
		{
			using OleDbSchema oleDbSchema = new(databaseFile);
			Collection<Table> tables = oleDbSchema.GetSchema();

			return tables;
		}

		/// <summary>
		/// Creates a file with the given schema.
		/// </summary>
		/// <param name="schemaFile">The schema file.</param>
		/// <param name="databaseFile">The database file.</param>
		/// <returns>A values indicating success or not.</returns>
		public static bool ImportSchema(
			string schemaFile, string databaseFile)
		{
			bool successCode = false;

			if (!File.Exists(schemaFile))
			{
				string message = $"Schema file not found: {schemaFile}";
				throw new FileNotFoundException(message, schemaFile);
			}
			else
			{
				try
				{
					string fileContents = File.ReadAllText(schemaFile);
					IReadOnlyList<string> queries =
						DataStoreStructure.GetSqlQueryStatements(fileContents);

					bool isValid =
						OleDbHelper.ValidateAccessDatabaseFile(databaseFile);

					if (isValid == true)
					{
						string connectionString =
							OleDbHelper.BuildConnectionString(databaseFile);

						using DataStorageOleDb database = new(connectionString);

						successCode =
							DataDefinition.ExecuteNonQueries(database, queries);
					}
				}
				catch (Exception exception) when
					(exception is ArgumentNullException ||
					exception is ArgumentException ||
					exception is FileNotFoundException ||
					exception is DirectoryNotFoundException ||
					exception is IOException ||
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

			return successCode;
		}
	}
}
