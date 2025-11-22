// <copyright file="BaseTestsSupport.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests
{
	using NUnit.Framework;
	using NUnit.Framework.Internal;
	using System.Data.Common;
	using System.IO;

	/// <summary>
	/// Base test support class.
	/// </summary>
	[SetUpFixture]
	internal class BaseTestsSupport
	{
		/// <summary>
		/// database storage object.
		/// </summary>
		private DataStorage database;

		/// <summary>
		/// Gets the data storage instance used by the application for database operations.
		/// </summary>
		public DataStorage Database
		{
			get { return database; }
		}

		/// <summary>
		/// The one time setup method.
		/// </summary>
		[OneTimeSetUp]
		public void BaseOneTimeSetUp()
		{
			SetupSchema();
		}

		/// <summary>
		/// One time tear down method.
		/// </summary>
		[OneTimeTearDown]
		public void BaseOneTimeTearDown()
		{
		}

		private void SetupSchema()
		{
			string statement = "CREATE TABLE TestTable " +
				"(id INTEGER PRIMARY KEY, description VARCHAR(64))";

			bool result = database.ExecuteNonQuery(statement);
			Assert.That(result, Is.True);

			statement = "SELECT name FROM sqlite_master " +
				"WHERE type = 'table' AND name = 'TestTable';";

			using DbDataReader dbDataReader = database.ExecuteReader(statement);

			Assert.That(dbDataReader.HasRows, Is.True);
		}
	}
}
