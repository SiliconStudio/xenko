// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Possible level of logging used by <see cref="IPackagesLogger"/>.
    /// </summary>
    public enum MessageLevel
    {
        Debug,
        Verbose,
        Info,
        Minimal,
        Warning,
        Error,
        InfoSummary,
        ErrorSummary
    }
}