// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Null implementation of <see cref="IPackagesLogger"/>.
    /// </summary>
    public class NullPackagesLogger : IPackagesLogger
    {
        public static IPackagesLogger Instance { get; } = new NullPackagesLogger();

        public void Log(MessageLevel level, string message)
        {
        }
    }
}
