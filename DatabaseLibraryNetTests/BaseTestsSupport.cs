// <copyright file="BaseTestsSupport.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests
{
	using System;
	using System.Data.SQLite;
	using System.Globalization;
	using System.IO;
	using DigitalZenWorks.Common.Utilities;
	using NUnit.Framework;

	/// <summary>
	/// Base test support class.
	/// </summary>
	internal abstract class BaseTestsSupport : IDisposable
	{
		/// <summary>
		/// database storage object.
		/// </summary>
		private DataStorage database;
		private string dataSource;
		private bool disposed;

		/// <summary>
		/// Gets the data storage instance used by the application for database operations.
		/// </summary>
		public DataStorage Database
		{
			get { return database; }
		}

		/// <summary>
		/// Gets the data source used by the application for database operations.
		/// </summary>
		public string DataSource
		{
			get { return dataSource; }
		}

		/// <summary>
		/// The one time setup method.
		/// </summary>
		[OneTimeSetUp]
		public void BaseOneTimeSetUp()
		{
			GetDatabase();
			SetupSchema();
		}

		/// <summary>
		/// One time tear down method.
		/// </summary>
		[OneTimeTearDown]
		public void BaseOneTimeTearDown()
		{
			if (database != null)
			{
				database.Close();
				database.Shutdown();
			}

			File.Delete(dataSource);
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose method.
		/// </summary>
		/// <param name="disposing">True to release both managed and unmanaged
		/// resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing == true && disposed == false)
			{
				database?.Close();
				database = null;
			}

			disposed = true;
		}

		private static string GetEmbeddedResourceFile(
			string resource, string extension)
		{
			string fileName = Path.GetTempFileName();

			// A 0 byte sized file is created.  Need to remove it.
			File.Delete(fileName);
			string filePath = Path.ChangeExtension(fileName, extension);

			bool result = FileUtils.CreateFileFromEmbeddedResource(
				resource, filePath);

			Assert.That(result, Is.True);

			result = File.Exists(filePath);
			Assert.That(result, Is.True);

			return filePath;
		}

		private static string GetTestDatabasePath()
		{
			string fileName = Path.GetTempFileName();

			// A 0 byte sized file is created.  Need to remove it.
			File.Delete(fileName);
			string databasePath = Path.ChangeExtension(fileName, ".db");

			return databasePath;
		}

		private static string GetTestSqlFile()
		{
			string resource = "DigitalZenWorks.Database.ToolKit.Tests." +
				"Products.Sqlite.Test.sql";

			string filePath = GetEmbeddedResourceFile(resource, "sql");

			return filePath;
		}

		private void GetDatabase()
		{
			dataSource = GetTestDatabasePath();

			SQLiteConnection.CreateFile(dataSource);

			string connectionBase = "Data Source={0};Version=3;" +
				"DateTimeFormat=InvariantCulture";

			string connectionString = string.Format(
				CultureInfo.InvariantCulture,
				connectionBase,
				dataSource);

			database = new DataStorage(DatabaseType.SQLite, connectionString);
		}

		private void SetupSchema()
		{
			string sqlFile = GetTestSqlFile();
			string sql = File.ReadAllText(sqlFile);

			bool result = database.ExecuteNonQuery(sql);
			Assert.That(result, Is.True);
		}
	}
}
