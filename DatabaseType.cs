/////////////////////////////////////////////////////////////////////////////
// <copyright file="DatabaseType.cs" company="James John McGuire">
// Copyright © 2006 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Represents values to specify the types of databases.
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public enum DatabaseType
	{
		/// <summary>
		/// OleDb
		/// </summary>
		OleDb,

		/// <summary>
		/// SqlServer
		/// </summary>
		SqlServer,

		/// <summary>
		/// Oracle
		/// </summary>
		Oracle,

		/// <summary>
		/// MySql
		/// </summary>
		MySql
	}
}
