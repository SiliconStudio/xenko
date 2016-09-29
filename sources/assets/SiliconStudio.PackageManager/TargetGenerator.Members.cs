// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using NuGet;

namespace SiliconStudio.PackageManager
{
    partial class TargetGenerator
    {
        private readonly NugetStore store;
        private readonly List<IPackage> packages;

        internal TargetGenerator(NugetStore store, List<IPackage> packages)
        {
            this.store = store;
            this.packages = packages;
        }
    }
}