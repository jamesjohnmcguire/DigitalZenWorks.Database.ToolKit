using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	// A struct to hold foreign key constraints temporarily
	internal struct Relationship
	{
		public string Name;
		public string ParentTable;
		public string ParentTableCol;
		public string ChildTable;
		public string ChildTableCol;
		public bool OnUpdateCascade;
		public bool OnDeleteCascade;
	}
}
