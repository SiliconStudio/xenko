// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.PackageManager
{
    /// <summary>
    /// Generic interface for logging. See <see cref="MessageLevel"/> for various level of logging.
    /// </summary>
    public interface IPackageManagerLogger
    {
        void Log(MessageLevel level, string message);
    }
}
