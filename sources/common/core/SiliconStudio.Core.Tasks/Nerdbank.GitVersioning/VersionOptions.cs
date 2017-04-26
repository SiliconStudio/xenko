// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;

namespace Nerdbank.GitVersioning
{
    /// <summary>
    /// Store package version read from .xkpkg, implemented for <see cref="GitExtensions"/>.
    /// </summary>
    class VersionOptions
    {
        public int BuildNumberOffset => 0;

        public PackageVersion Version { get; set; }
    }
}
