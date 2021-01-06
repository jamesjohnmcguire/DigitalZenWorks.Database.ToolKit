/////////////////////////////////////////////////////////////////////////////
// <copyright file="IDatabaseCollection.cs" company="James John McGuire">
// Copyright © 2006 - 2021 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	interface IDatabaseCollection : IDisposable
	{
		int GetIdFromName(string name);
	}
}
