// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;

namespace SiliconStudio.Packages
{
    partial class TargetGenerator
    {
        private readonly NugetStore store;
        private readonly List<NugetPackage> packages;

        internal TargetGenerator(NugetStore store, List<NugetPackage> packages)
        {
            this.store = store;
            this.packages = packages;
        }
    }
}
