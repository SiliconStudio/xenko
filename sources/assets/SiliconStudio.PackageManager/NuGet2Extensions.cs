// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using NuGet;

namespace SiliconStudio.PackageManager
{
    internal static class NuGet2Extensions
    {
        /// <summary>
        /// Given a <see cref="ConstraintProvider"/> construct a NuGet equivalent.
        /// </summary>
        /// <param name="provider">The provider to convert.</param>
        /// <returns>An instance of conforming type <see cref="IPackageConstraintProvider"/> matching <paramref name="provider"/>.</returns>
        public static IPackageConstraintProvider Provider (this ConstraintProvider provider)
        {
            if ((provider == null) || (!provider.HasConstraints))
            {
                return NullConstraintProvider.Instance;
            }
            else
            {
                var res = new DefaultConstraintProvider();
                foreach (var constraint in provider.Constraints)
                {
                    res.AddConstraint(constraint.Key, constraint.Value.ToVersionSpec().VersionSpec);
                }
                return res;
            }
        }

        public static PackageVersionRange ToPackageVersionRange(this IVersionSpec version)
        {
            PackageVersion min = null, max = null;
            if (version.MinVersion?.Version != null)
            {
                min = new PackageVersion(version.MinVersion.Version);
            }
            if (version.MaxVersion?.Version != null)
            {
                max = new PackageVersion(version.MaxVersion.Version);
            }
            return new PackageVersionRange(min, version.IsMinInclusive, max, version.IsMaxInclusive);
        }
    }
}
