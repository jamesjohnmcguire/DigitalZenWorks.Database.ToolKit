/////////////////////////////////////////////////////////////////////////////
// $Id: DatabaseTypes.cs 37 2015-05-27 05:42:58Z JamesMc $
//
// Copyright (c) 2006-2015 by James John McGuire
// All rights reserved.
/////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zenware.DatabaseLibrary
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Represents values to specify the types of databases
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public enum DatabaseTypes
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
