// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

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

        public static Func<T, bool> ToFilter<T>(this PackageVersionRange versionInfo, Func<T, PackageVersion> extractor)
        {
            if (versionInfo == null)
            {
                throw new ArgumentNullException("versionInfo");
            }
            if (extractor == null)
            {
                throw new ArgumentNullException("extractor");
            }

            return p =>
            {
                PackageVersion version = extractor(p);
                bool condition = true;
                if (versionInfo.MinVersion != null)
                {
                    if (versionInfo.IsMinInclusive)
                    {
                        condition = version >= versionInfo.MinVersion;
                    }
                    else
                    {
                        condition = version > versionInfo.MinVersion;
                    }
                }

                if (versionInfo.MaxVersion != null)
                {
                    if (versionInfo.IsMaxInclusive)
                    {
                        condition = condition && version <= versionInfo.MaxVersion;
                    }
                    else
                    {
                        condition = condition && version < versionInfo.MaxVersion;
                    }
                }

                return condition;
            };
        }         
    }
}