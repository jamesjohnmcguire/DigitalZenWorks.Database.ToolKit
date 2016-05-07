using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalZenWorks.Common.DatabaseLibrary
{
	/// <summary>
	/// A class to hold foreign key constraints temporarily
	/// </summary>
	public class Relationship
	{
		private string child;
		private string childColumn;
		private bool deleteCascade;
		private string name;
		private string parent;
		private string parentColumn;
		private bool updateCascade;

		/// <summary>
		/// Name
		/// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}
		/// <summary>
		/// Parent Table
		/// </summary>
		public string ParentTable
		{
			get { return parent; }
			set { parent = value; }
		}
		/// <summary>
		/// Parent Table Column
		/// </summary>
		public string ParentTableCol
		{
			get { return parentColumn; }
			set { parentColumn = value; }
		}
		/// <summary>
		/// Child Table
		/// </summary>
		public string ChildTable
		{
			get { return child; }
			set { child = value; }
		}
		/// <summary>
		/// Child Table Column
		/// </summary>
		public string ChildTableCol
		{
			get { return childColumn; }
			set { childColumn = value; }
		}
		/// <summary>
		/// Indicates whether update cascades
		/// </summary>
		public bool OnUpdateCascade
		{
			get { return updateCascade; }
			set { updateCascade = value; }
		}
		/// <summary>
		/// Indicates whether delete cascades
		/// </summary>
		public bool OnDeleteCascade
		{
			get { return deleteCascade; }
			set { deleteCascade = value; }
		}
	}
}
