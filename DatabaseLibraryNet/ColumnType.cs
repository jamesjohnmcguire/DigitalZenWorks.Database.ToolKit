﻿/////////////////////////////////////////////////////////////////////////////
// <copyright file="ColumnType.cs" company="James John McGuire">
// Copyright © 2006 - 2022 James John McGuire. All Rights Reserved.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit
{
	/// <summary>
	/// Generic column types
	/// Represents an enumeration of column types.
	///
	/// TODO: Update to be less Access and more generic.
	/// </summary>
	public enum ColumnType
	{
		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// Auto increment type
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		AutoNumber,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		BigInt,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Binary,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Bit,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Blob,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access (Yes/No), MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Boolean,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Byte,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access, MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Char,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		///  Currency type
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Currency,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Cursor,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Date,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// DateTime type
		/// MySQL, SQLServer, MS ACCESS (as Date/Time)
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		DateTime,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		DateTime2,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		DateTimeOffset,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Decimal,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access, MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Double,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Hyperlink,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Enum,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Float,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Identity,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Image,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Int,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Integer,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Other
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		JavaObject,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Long,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		LongBlob,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		LongText,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		LongVarBinary,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		LongVarChar,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		LookupWizard,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		MediumBlob,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		MediumInt,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		MediumText,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		///  Memo (large text) type
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Memo,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Money,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		NChar,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		NText,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Number (integer) type
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Number,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Numeric,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		NVarChar,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		OleObject,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		///  Ole object type
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Ole,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Other column type
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Other,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Real,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Set,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Single,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		SmallDateTime,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		SmallInt,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		SmallMoney,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		SqlVariant,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// String type
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		String,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Table,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MS Access, MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Text,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Time,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Timestamp,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		TinyInt,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		TinyText,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		UniqueIdentifier,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		VarBinary,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL, SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		VarChar,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// SQLServer
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Xml,

		/////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MySQL
		/// </summary>
		/////////////////////////////////////////////////////////////////////
		Year,

		/// <summary>
		///  Boolean
		/// </summary>
		YesNo
	}
}