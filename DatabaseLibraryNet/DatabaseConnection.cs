/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseConnection.cs" company="James John McGuire">
// Copyright © 2006 - 2025 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// The database connection class.
	/// </summary>
	public class DatabaseConnection : IDisposable
	{
		private string connectionString;

		/// <summary>
		/// Initializes a new instance of the <see cref="DatabaseConnection"/>
		/// class.
		/// </summary>
		/// <param name="connectionString">The connection string
		/// to use.</param>
		public DatabaseConnection(string connectionString)
		{
			this.connectionString = connectionString;
		}

		/// <summary>
		/// The dispose method.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// The dispose method.
		/// </summary>
		/// <param name="disposing">Indicates whether it is
		/// currently disposing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
			}
		}
	}
}
