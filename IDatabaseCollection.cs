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
