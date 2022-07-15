/////////////////////////////////////////////////////////////////////////////
// <copyright file="Column.cs" company="James John McGuire">
// Copyright © 2006 - 2022 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	/////////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Represents a database column.
	/// </summary>
	/////////////////////////////////////////////////////////////////////////
	public class Column
	{
		private string defaultValue = string.Empty;
		private int length = 255;
		private string name = string.Empty;
		private bool nullable;
		private int position = 1;
		private ColumnType type = ColumnType.Number;
		private bool unique;

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="Column"/> class.
		/// Default Constructor.
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		public Column()
		{
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes a new instance of the <see cref="Column"/> class.
		/// Constructor.
		/// </summary>
		/// <param name="name">The name of the column.</param>
		/// <param name="type">The type of the column.</param>
		/// <param name="length">The length of the column.</param>
		/// <param name="unique">Indicates whether the column is unique.</param>
		/// <param name="nullable">Indicates whether the column is nullable.</param>
		/// <param name="defaultValue">The default value of the column.</param>
		/// <param name="position">The position of the column.</param>
		/////////////////////////////////////////////////////////////////////
		public Column(
			string name,
			ColumnType type,
			int length,
			bool unique,
			bool nullable,
			string defaultValue,
			int position)
		{
			Name = name;
			ColumnType = type;
			Length = length;
			Unique = unique;
			Nullable = nullable;
			DefaultValue = defaultValue;
			Position = position;
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets the type of the column.
		/// </summary>
		/// <value>
		/// The type of the column.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public ColumnType ColumnType
		{
			get { return type; }
			set { type = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets represents the default value of the column.
		/// </summary>
		/// <value>
		/// Represents the default value of the column.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public string DefaultValue
		{
			get { return defaultValue; }
			set { defaultValue = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets a value indicating whether indicates whether the column is indexed or not.
		/// </summary>
		/// <value>
		/// A value indicating whether indicates whether the column is indexed or not.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public bool Indexed { get; set; }

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets the length of the column.
		/// </summary>
		/// <value>
		/// The length of the column.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public int Length
		{
			get { return length; }
			set { length = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets indicates the column's maximum value allowed.
		/// </summary>
		/// <value>
		/// Indicates the column's maximum value allowed.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public int MaxValue { get; set; }

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets indicates the column's minimum value allowed.
		/// </summary>
		/// <value>
		/// Indicates the column's minimum value allowed.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public int MinValue { get; set; }

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets the name of the column.
		/// </summary>
		/// <value>
		/// The name of the column.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets a value indicating whether represents whether the column can have a null value.
		/// </summary>
		/// <value>
		/// A value indicating whether represents whether the column can have a null value.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public bool Nullable
		{
			get { return nullable; }
			set { nullable = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets represents the position of the column.
		/// </summary>
		/// <value>
		/// Represents the position of the column.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public int Position
		{
			get { return position; }
			set { position = value; }
		}

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets a value indicating whether indicates whether the column is the primary column.
		/// </summary>
		/// <value>
		/// A value indicating whether indicates whether the column is the primary column.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public bool Primary { get; set; }

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets or sets a value indicating whether represents whether the column requires unique values.
		/// </summary>
		/// <value>
		/// A value indicating whether represents whether the column requires unique values.
		/// </value>
		/////////////////////////////////////////////////////////////////////
		public bool Unique
		{
			get { return unique; }
			set { unique = value; }
		}
	}
}
