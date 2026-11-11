// <copyright file="DatabaseFileInspectorTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

/// <summary>
/// Tests file identification and the retained DataDefinition entry points.
/// </summary>
[TestFixture]
internal sealed class DatabaseFileInspectorTests
{
	private string testDirectory;

	/// <summary>
	/// Creates an isolated directory for file inspection tests.
	/// </summary>
	[SetUp]
	public void Setup()
	{
		testDirectory = Path.Combine(
			Path.GetTempPath(), "DatabaseFileInspectorTests-" + Guid.NewGuid());
		Directory.CreateDirectory(testDirectory);
	}

	/// <summary>
	/// Removes test files after all inspection streams have been disposed.
	/// </summary>
	[TearDown]
	public void TearDown()
	{
		if (Directory.Exists(testDirectory))
		{
			Directory.Delete(testDirectory, true);
		}
	}

	/// <summary>
	/// Recognizes each supported signature without changing the input bytes.
	/// </summary>
	/// <param name="format">The signature to inspect.</param>
	/// <param name="expected">The expected database type.</param>
	[TestCase("SQLite", DatabaseType.SQLite)]
	[TestCase("ACE", DatabaseType.OleDb)]
	[TestCase("Jet", DatabaseType.OleDb)]
	[TestCase("SqlServer", DatabaseType.SqlServer)]
	public void HeaderIdentifiesSupportedFormat(
		string format, DatabaseType expected)
	{
		byte[] header = GetSignature(format);
		byte[] original = (byte[])header.Clone();

		AssertHeaderType(header, expected);
		Assert.That(header, Is.EqualTo(original));

		byte[] longerHeader = new byte[64];
		header.CopyTo(longerHeader, 0);
		longerHeader[63] = 0xFF;
		AssertHeaderType(longerHeader, expected);
	}

	/// <summary>
	/// Partial signatures return Unknown rather than reading past the buffer.
	/// </summary>
	/// <param name="format">The signature to truncate.</param>
	[TestCase("SQLite")]
	[TestCase("ACE")]
	[TestCase("Jet")]
	[TestCase("SqlServer")]
	public void TruncatedHeaderReturnsUnknown(string format)
	{
		byte[] signature = GetSignature(format);
		for (int length = 0; length < signature.Length; length++)
		{
			byte[] header = new byte[length];
			Array.Copy(signature, header, length);
			AssertHeaderType(header, DatabaseType.Unknown);
		}
	}

	/// <summary>
	/// Incorrect signature bytes prevent header recognition.
	/// </summary>
	/// <param name="format">The signature to damage.</param>
	/// <param name="position">The byte to replace.</param>
	[TestCase("SQLite", 0)]
	[TestCase("SQLite", 7)]
	[TestCase("ACE", 0)]
	[TestCase("ACE", 14)]
	[TestCase("Jet", 0)]
	[TestCase("Jet", 15)]
	[TestCase("SqlServer", 0)]
	[TestCase("SqlServer", 1)]
	public void DamagedHeaderReturnsUnknown(string format, int position)
	{
		byte[] header = GetSignature(format);
		header[position] = 0xFF;
		AssertHeaderType(header, DatabaseType.Unknown);
	}

	/// <summary>
	/// Missing or unrecognized metadata is not mistaken for a database type.
	/// </summary>
	[Test]
	public void NullAndUnrecognizedHeadersReturnUnknown()
	{
		AssertHeaderType(null, DatabaseType.Unknown);
		AssertHeaderType(new byte[32], DatabaseType.Unknown);
		AssertHeaderType(
			Encoding.ASCII.GetBytes("This is an ordinary text file."),
			DatabaseType.Unknown);
	}

	/// <summary>
	/// Extension detection supports aliases and case without accessing files.
	/// </summary>
	/// <param name="fileName">A nonexistent file name.</param>
	/// <param name="expected">The expected database type.</param>
	[TestCase("sample.db", DatabaseType.SQLite)]
	[TestCase("sample.sqlite", DatabaseType.SQLite)]
	[TestCase("sample.sqlite3", DatabaseType.SQLite)]
	[TestCase("sample.db3", DatabaseType.SQLite)]
	[TestCase("sample.sdb", DatabaseType.SQLite)]
	[TestCase("sample.mdb", DatabaseType.OleDb)]
	[TestCase("sample.accdb", DatabaseType.OleDb)]
	[TestCase("sample.mdf", DatabaseType.SqlServer)]
	[TestCase("sample.SQLITE3", DatabaseType.SQLite)]
	[TestCase("sample.AcCdB", DatabaseType.OleDb)]
	[TestCase("sample.MDF", DatabaseType.SqlServer)]
	[TestCase("sample.accdb.bak", DatabaseType.Unknown)]
	[TestCase("sample.txt", DatabaseType.Unknown)]
	[TestCase("sample", DatabaseType.Unknown)]
	[TestCase("sample.", DatabaseType.Unknown)]
	[TestCase("", DatabaseType.Unknown)]
	public void ExtensionIdentifiesTypeWithoutRequiringAFile(
		string fileName, DatabaseType expected)
	{
		string path = Path.Combine(testDirectory, fileName);
		Assert.That(File.Exists(path), Is.False);
		Assert.That(
			DatabaseFileInspector.GetDatabaseTypeByExtension(path),
			Is.EqualTo(expected));
#pragma warning disable CS0618 // Verify the retained public entry point.
		Assert.That(
			DataDefinition.GetDatabaseTypeByExtension(path),
			Is.EqualTo(expected));
#pragma warning restore CS0618
	}

	/// <summary>
	/// Identification requires an existing file even with a known extension.
	/// </summary>
	[Test]
	public void MissingPathsAndDirectoriesReturnUnknown()
	{
		AssertFileType(null, DatabaseType.Unknown);
		AssertFileType(string.Empty, DatabaseType.Unknown);
		AssertFileType(testDirectory, DatabaseType.Unknown);
		AssertFileType(
			Path.Combine(testDirectory, "missing.accdb"), DatabaseType.Unknown);
	}

	/// <summary>
	/// Recognized headers take precedence over conflicting file extensions.
	/// </summary>
	/// <param name="format">The signature to write.</param>
	/// <param name="fileName">The conflicting or unknown file name.</param>
	/// <param name="expected">The expected database type.</param>
	[TestCase("SQLite", "sample.accdb", DatabaseType.SQLite)]
	[TestCase("ACE", "sample.sqlite", DatabaseType.OleDb)]
	[TestCase("Jet", "sample.mdf", DatabaseType.OleDb)]
	[TestCase("SqlServer", "sample.mdb", DatabaseType.SqlServer)]
	[TestCase("SQLite", "sample", DatabaseType.SQLite)]
	[TestCase("ACE", "sample.bin", DatabaseType.OleDb)]
	public void FileHeaderTakesPrecedenceOverExtension(
		string format, string fileName, DatabaseType expected)
	{
		string path = Path.Combine(testDirectory, fileName);
		File.WriteAllBytes(path, GetSignature(format));
		AssertFileType(path, expected);
	}

	/// <summary>
	/// Unknown or absent header bytes fall back to the file extension.
	/// </summary>
	/// <param name="fileName">The file name to inspect.</param>
	/// <param name="expected">The expected database type.</param>
	[TestCase("sample.accdb", DatabaseType.OleDb)]
	[TestCase("sample.mdb", DatabaseType.OleDb)]
	[TestCase("sample.sqlite3", DatabaseType.SQLite)]
	[TestCase("sample.mdf", DatabaseType.SqlServer)]
	[TestCase("sample.bin", DatabaseType.Unknown)]
	[TestCase("sample", DatabaseType.Unknown)]
	public void UnknownAndEmptyFilesUseExtensionFallback(
		string fileName, DatabaseType expected)
	{
		string path = Path.Combine(testDirectory, fileName);
		File.WriteAllBytes(path, []);
		AssertFileType(path, expected);
		File.WriteAllBytes(path, [0xFF, 0xFE, 0xFD]);
		AssertFileType(path, expected);
	}

	/// <summary>
	/// Header reading returns only the first 32 bytes, padding short files.
	/// </summary>
	/// <param name="fileLength">The number of bytes in the source file.</param>
	[TestCase(0)]
	[TestCase(3)]
	[TestCase(16)]
	[TestCase(32)]
	[TestCase(64)]
	public void HeaderReadReturnsFirst32BytesWithZeroPadding(int fileLength)
	{
		byte[] contents = new byte[fileLength];
		for (int index = 0; index < contents.Length; index++)
		{
			contents[index] = (byte)(index + 1);
		}

		byte[] expected = new byte[32];
		Array.Copy(contents, expected, Math.Min(contents.Length, 32));
		string path = Path.Combine(testDirectory, "sample.bin");
		File.WriteAllBytes(path, contents);
		AssertHeaderBytes(path, expected);

		using (FileStream stream = new(
			path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
		{
			Assert.That(stream.Length, Is.EqualTo(fileLength));
		}

		Assert.That(File.ReadAllBytes(path), Is.EqualTo(contents));
	}

	/// <summary>
	/// Files open for shared writes can still be inspected.
	/// </summary>
	[Test]
	public void HeaderCanBeReadWhileAnotherStreamHasTheFileOpen()
	{
		string path = Path.Combine(testDirectory, "shared.bin");
		byte[] expected = new byte[32];
		expected[0] = 42;
		File.WriteAllBytes(path, expected);
		using FileStream stream = new(
			path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

		AssertHeaderBytes(path, expected);
		Assert.That(stream.CanWrite, Is.True);
	}

	/// <summary>
	/// Null paths retain their logged failure result rather than throwing.
	/// </summary>
	[Test]
	public void NullHeaderFilePathReturnsNull()
	{
		AssertHeaderBytes(null, null);
	}

	/// <summary>
	/// Direct header reads report missing files rather than creating them.
	/// </summary>
	[Test]
	public void MissingHeaderFileThrowsFileNotFoundException()
	{
		string path = Path.Combine(testDirectory, "missing.accdb");
		Assert.That(
			() => DatabaseFileInspector.GetDatabaseFileHeaderBytes(path),
			Throws.TypeOf<FileNotFoundException>());
#pragma warning disable CS0618 // Verify the retained public entry point.
		Assert.That(
			() => DataDefinition.GetDatabaseFileHeaderBytes(path),
			Throws.TypeOf<FileNotFoundException>());
#pragma warning restore CS0618
		Assert.That(File.Exists(path), Is.False);
	}

	/// <summary>
	/// Empty paths preserve the direct header reader's argument exception.
	/// </summary>
	[Test]
	public void EmptyHeaderFilePathThrowsArgumentException()
	{
		Assert.That(
			() => DatabaseFileInspector.GetDatabaseFileHeaderBytes(string.Empty),
			Throws.TypeOf<ArgumentException>());
#pragma warning disable CS0618 // Verify the retained public entry point.
		Assert.That(
			() => DataDefinition.GetDatabaseFileHeaderBytes(string.Empty),
			Throws.TypeOf<ArgumentException>());
#pragma warning restore CS0618
	}

	/// <summary>
	/// The legacy methods remain public with non-error migration warnings.
	/// </summary>
	/// <param name="methodName">The retained method name.</param>
	/// <param name="parameterType">Its parameter type.</param>
	[TestCase("GetDatabaseType", typeof(string))]
	[TestCase("GetDatabaseTypeByExtension", typeof(string))]
	[TestCase("GetDatabaseTypeByFileHeaderBytes", typeof(byte[]))]
	[TestCase("GetDatabaseFileHeaderBytes", typeof(string))]
	public void LegacyMethodsRetainObsoleteMigrationWarnings(
		string methodName, Type parameterType)
	{
		MethodInfo method = typeof(DataDefinition).GetMethod(
			methodName, [parameterType]);
		Assert.That(method, Is.Not.Null);
		ObsoleteAttribute obsolete =
			method.GetCustomAttribute<ObsoleteAttribute>();
		Assert.That(obsolete, Is.Not.Null);
		Assert.That(obsolete.IsError, Is.False);
		Assert.That(
			obsolete.Message, Does.Contain("DatabaseFileInspector." + methodName));
	}

	private static void AssertFileType(string path, DatabaseType expected)
	{
		Assert.That(
			DatabaseFileInspector.GetDatabaseType(path), Is.EqualTo(expected));
#pragma warning disable CS0618 // Verify the retained public entry point.
		Assert.That(DataDefinition.GetDatabaseType(path), Is.EqualTo(expected));
#pragma warning restore CS0618
	}

	private static void AssertHeaderType(byte[] header, DatabaseType expected)
	{
		Assert.That(
			DatabaseFileInspector.GetDatabaseTypeByFileHeaderBytes(header),
			Is.EqualTo(expected));
#pragma warning disable CS0618 // Verify the retained public entry point.
		Assert.That(
			DataDefinition.GetDatabaseTypeByFileHeaderBytes(header),
			Is.EqualTo(expected));
#pragma warning restore CS0618
	}

	private static void AssertHeaderBytes(string path, byte[] expected)
	{
		Assert.That(
			DatabaseFileInspector.GetDatabaseFileHeaderBytes(path),
			Is.EqualTo(expected));
#pragma warning disable CS0618 // Verify the retained public entry point.
		Assert.That(
			DataDefinition.GetDatabaseFileHeaderBytes(path), Is.EqualTo(expected));
#pragma warning restore CS0618
	}

	private static byte[] GetSignature(string format)
	{
		byte[] signature = format switch
		{
			"SQLite" => Encoding.ASCII.GetBytes("SQLite format 3\0"),
			"ACE" =>
			[
				0, 1, 0, 0, .. Encoding.ASCII.GetBytes("Standard ACE DB\0")
			],
			"Jet" =>
			[
				0, 1, 0, 0, .. Encoding.ASCII.GetBytes("Standard Jet DB\0")
			],
			"SqlServer" => [1, 15, 0, 0],
			_ => throw new ArgumentException("Unknown test format.", nameof(format)),
		};

		return signature;
	}
}
