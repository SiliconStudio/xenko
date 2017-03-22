// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    public static class PackageVersionRangeExtensions
    {
        public static Func<Package, bool> ToFilter(this PackageVersionRange versionInfo)
        {
            if (versionInfo == null)
            {
                throw new ArgumentNullException("versionInfo");
            }
            return versionInfo.ToFilter<Package>(p => p.Meta.Version);
        }

    }
}