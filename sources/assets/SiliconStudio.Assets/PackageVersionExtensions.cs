// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Assets
{
    internal static class PackageVersionExtensions
    {

        public static NuGet.SemanticVersion ToSemanticVersion(this PackageVersion version)
        {
            return version == null ? null : new NuGet.SemanticVersion(version.ToString());
        }


        public static NuGet.IVersionSpec ToVersionSpec(this PackageVersionRange versionRange)
        {
            return versionRange == null
                ? null
                : new NuGet.VersionSpec()
                    {
                        MinVersion = versionRange.MinVersion != null ? versionRange.MinVersion.ToSemanticVersion() : null,
                        IsMinInclusive = versionRange.IsMinInclusive,
                        MaxVersion = versionRange.MaxVersion != null ? versionRange.MaxVersion.ToSemanticVersion() : null,
                        IsMaxInclusive = versionRange.IsMaxInclusive
                    };
        }
    }
}