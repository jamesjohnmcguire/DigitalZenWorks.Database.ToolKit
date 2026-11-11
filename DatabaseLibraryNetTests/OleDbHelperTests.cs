// <copyright file="OleDbHelperTests.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>

namespace DigitalZenWorks.Database.ToolKit.Tests;

using System;
using System.IO;
using System.Runtime.Versioning;
using System.Text;
using NUnit.Framework;

/// <summary>
/// Tests Access file identification without opening a database connection.
/// </summary>
[SupportedOSPlatform("windows")]
[TestFixture]
internal sealed class OleDbHelperTests
{
	private string testDirectory;

	/// <summary>
	/// Creates an isolated directory for each validation case.
	/// </summary>
	[SetUp]
	public void Setup()
	{
		testDirectory = Path.Combine(
			Path.GetTempPath(), "OleDbHelperTests-" + Guid.NewGuid());
		Directory.CreateDirectory(testDirectory);
	}

	/// <summary>
	/// Removes test files after inspection streams have closed.
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
	/// Missing paths return false even when the extension suggests Access.
	/// </summary>
	/// <param name="fileName">The null, empty, or missing file name.</param>
	[TestCase(null)]
	[TestCase("")]
	[TestCase("missing.accdb")]
	[TestCase("missing.mdb")]
	public void MissingFileReturnsFalse(string fileName)
	{
		string path = fileName;
		if (!string.IsNullOrEmpty(fileName))
		{
			path = Path.Combine(testDirectory, fileName);
		}

		Assert.That(OleDbHelper.ValidateAccessDatabaseFile(path), Is.False);
	}

	/// <summary>
	/// An existing directory is not treated as a database file.
	/// </summary>
	[Test]
	public void DirectoryReturnsFalse()
	{
		string path = Path.Combine(testDirectory, "directory.accdb");
		Directory.CreateDirectory(path);
		Assert.That(OleDbHelper.ValidateAccessDatabaseFile(path), Is.False);
	}

	/// <summary>
	/// Access headers are accepted even with conflicting or absent extensions.
	/// </summary>
	/// <param name="format">The Access header format.</param>
	/// <param name="fileName">The source file name.</param>
	[TestCase("ACE", "sample.accdb")]
	[TestCase("Jet", "sample.mdb")]
	[TestCase("ACE", "sample.sqlite")]
	[TestCase("Jet", "sample.mdf")]
	[TestCase("ACE", "sample")]
	public void AccessHeaderReturnsTrue(string format, string fileName)
	{
		string path = Path.Combine(testDirectory, fileName);
		byte[] header = GetSignature(format);
		File.WriteAllBytes(path, header);

		Assert.That(OleDbHelper.ValidateAccessDatabaseFile(path), Is.True);
		Assert.That(File.ReadAllBytes(path), Is.EqualTo(header));
	}

	/// <summary>
	/// Unknown or empty files retain the Access extension fallback.
	/// </summary>
	/// <param name="fileName">The Access file name.</param>
	[TestCase("sample.accdb")]
	[TestCase("sample.mdb")]
	[TestCase("sample.AcCdB")]
	public void UnknownHeaderWithAccessExtensionReturnsTrue(string fileName)
	{
		string path = Path.Combine(testDirectory, fileName);
		File.WriteAllBytes(path, []);
		Assert.That(OleDbHelper.ValidateAccessDatabaseFile(path), Is.True);

		File.WriteAllBytes(path, [0xFF, 0xFE, 0xFD]);
		Assert.That(OleDbHelper.ValidateAccessDatabaseFile(path), Is.True);
	}

	/// <summary>
	/// A recognized non-Access header overrides an Access extension.
	/// </summary>
	/// <param name="format">The non-Access header format.</param>
	/// <param name="fileName">The misleading Access file name.</param>
	[TestCase("SQLite", "sample.accdb")]
	[TestCase("SQLite", "sample.mdb")]
	[TestCase("SqlServer", "sample.accdb")]
	[TestCase("SqlServer", "sample.mdb")]
	public void NonAccessHeaderThrowsArgumentException(
		string format, string fileName)
	{
		string path = Path.Combine(testDirectory, fileName);
		File.WriteAllBytes(path, GetSignature(format));
		AssertUnsupportedFile(path);
	}

	/// <summary>
	/// Existing unknown or non-Access files retain the rejection exception.
	/// </summary>
	/// <param name="fileName">The non-Access file name.</param>
	[TestCase("sample.sqlite")]
	[TestCase("sample.mdf")]
	[TestCase("sample.bin")]
	[TestCase("sample.accdb.bak")]
	[TestCase("sample")]
	public void UnknownHeaderWithoutAccessExtensionThrowsArgumentException(
		string fileName)
	{
		string path = Path.Combine(testDirectory, fileName);
		File.WriteAllBytes(path, []);
		AssertUnsupportedFile(path);

		File.WriteAllBytes(path, [0xFF, 0xFE, 0xFD]);
		AssertUnsupportedFile(path);
	}

	/// <summary>
	/// Inspection failures propagate instead of falling back to the extension.
	/// </summary>
	[Test]
	public void UnreadableHeaderThrowsIOException()
	{
		string path = Path.Combine(testDirectory, "locked.accdb");
		File.WriteAllBytes(path, GetSignature("ACE"));
		using (FileStream stream = new(
			path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
		{
			Assert.That(
				() => OleDbHelper.ValidateAccessDatabaseFile(path),
				Throws.InstanceOf<IOException>());
			Assert.That(stream.CanRead, Is.True);
		}

		Assert.That(OleDbHelper.ValidateAccessDatabaseFile(path), Is.True);
	}

	private static void AssertUnsupportedFile(string path)
	{
		ArgumentException exception = Assert.Throws<ArgumentException>(
			() => OleDbHelper.ValidateAccessDatabaseFile(path));
		Assert.That(exception.ParamName, Is.EqualTo("databaseFile"));
		Assert.That(
			exception.Message,
			Does.StartWith("Expected a Microsoft Access database file."));
	}

	private static byte[] GetSignature(string format)
	{
		byte[] signature = format switch
		{
			"ACE" =>
			[
				0, 1, 0, 0, .. Encoding.ASCII.GetBytes("Standard ACE DB\0")
			],
			"Jet" =>
			[
				0, 1, 0, 0, .. Encoding.ASCII.GetBytes("Standard Jet DB\0")
			],
			"SQLite" => Encoding.ASCII.GetBytes("SQLite format 3\0"),
			"SqlServer" => [1, 15, 0, 0],
			_ => throw new ArgumentException("Unknown test format.", nameof(format)),
		};

		return signature;
	}
}
