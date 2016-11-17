// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.PackageManager
{
    /// <summary>
    /// Null implementation of <see cref="IPackageManagerLogger"/>.
    /// </summary>
    public class NullPackageManagerLogger : IPackageManagerLogger
    {
        public static IPackageManagerLogger Instance { get; } = new NullPackageManagerLogger();

        public void Log(MessageLevel level, string message)
        {
        }
    }
}