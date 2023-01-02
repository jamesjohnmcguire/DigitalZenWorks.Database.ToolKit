/////////////////////////////////////////////////////////////////////////////
// <copyright file="Relationship.cs" company="James John McGuire">
// Copyright © 2006 - 2023 James John McGuire. All Rights Reserved.
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
	/// A class to hold foreign key constraints temporarily.
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
		/// Gets or sets name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets or sets parent Table.
		/// </summary>
		/// <value>The Parent Table.</value>
		public string ParentTable
		{
			get { return parent; }
			set { parent = value; }
		}

		/// <summary>
		/// Gets or sets parent Table Column.
		/// </summary>
		/// <value>The Parent Table Column.</value>
		public string ParentTableCol
		{
			get { return parentColumn; }
			set { parentColumn = value; }
		}

		/// <summary>
		/// Gets or sets child Table.
		/// </summary>
		/// <value>The Child Table.</value>
		public string ChildTable
		{
			get { return child; }
			set { child = value; }
		}

		/// <summary>
		/// Gets or sets child Table Column.
		/// </summary>
		/// <value>The Child Table Column.</value>
		public string ChildTableCol
		{
			get { return childColumn; }
			set { childColumn = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether indicates whether update cascades.
		/// </summary>
		/// <value>Indicates whether update cascades.</value>
		public bool OnUpdateCascade
		{
			get { return updateCascade; }
			set { updateCascade = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether indicates whether delete cascades.
		/// </summary>
		/// <value>Indicates whether delete cascades.</value>
		public bool OnDeleteCascade
		{
			get { return deleteCascade; }
			set { deleteCascade = value; }
		}
	}
}
