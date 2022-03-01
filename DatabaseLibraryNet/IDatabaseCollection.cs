/////////////////////////////////////////////////////////////////////////////
// <copyright file="IDatabaseCollection.cs" company="James John McGuire">
// Copyright © 2006 - 2022 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// Database collection interface.
	/// </summary>
	public interface IDatabaseCollection : IDisposable
	{
		/// <summary>
		/// Get id from name method.
		/// </summary>
		/// <param name="name">The name of item.</param>
		/// <returns>The id of the record.</returns>
		int GetIdFromName(string name);
	}
}
