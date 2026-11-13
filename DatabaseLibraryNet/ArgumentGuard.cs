/////////////////////////////////////////////////////////////////////////////
// <copyright file="ArgumentGuard.cs" company="Digital Zen Works">
// Copyright © 2006 - 2026 Digital Zen Works.
// </copyright>
/////////////////////////////////////////////////////////////////////////////

namespace DigitalZenWorks.Database.ToolKit;

using System;

/// <summary>
/// Provides argument validation across the supported target frameworks.
/// </summary>
internal static class ArgumentGuard
{
	/// <summary>
	/// Throws when the supplied argument is null.
	/// </summary>
	/// <param name="argument">The argument to validate.</param>
	/// <param name="parameterName">The original caller's parameter name.
	/// </param>
	/// <exception cref="ArgumentNullException">The argument is null.
	/// </exception>
	internal static void ThrowIfNull(object argument, string parameterName)
	{
#if NET6_0_OR_GREATER
		ArgumentNullException.ThrowIfNull(argument, parameterName);
#else
		if (argument == null)
		{
			throw new ArgumentNullException(parameterName);
		}
#endif
	}
}
