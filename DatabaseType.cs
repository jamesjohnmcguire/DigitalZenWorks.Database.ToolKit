/////////////////////////////////////////////////////////////////////////////
// $Id: DatabaseTypes.cs 38 2015-05-27 14:24:29Z JamesMc $
//
// Copyright © 2006-2016 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Represents values to specify the types of databases
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
