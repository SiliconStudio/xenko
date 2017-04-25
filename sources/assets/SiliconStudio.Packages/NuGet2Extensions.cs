// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Packages
{
    internal static class NuGet2Extensions
    {
        /// <summary>
        /// Given a <see cref="ConstraintProvider"/> construct a NuGet equivalent.
        /// </summary>
        /// <param name="provider">The provider to convert.</param>
        /// <returns>An instance of conforming type <see cref="NuGet.IPackageConstraintProvider"/> matching <paramref name="provider"/>.</returns>
        [NotNull]
        public static NuGet.IPackageConstraintProvider Provider ([NotNull] this ConstraintProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));

            if (!provider.HasConstraints)
            {
                return NuGet.NullConstraintProvider.Instance;
            }
            else
            {
                var res = new NuGet.DefaultConstraintProvider();
                foreach (var constraint in provider.Constraints)
                {
                    res.AddConstraint(constraint.Key, constraint.Value.ToVersionSpec());
                }
                return res;
            }
        }

        [NotNull]
        public static PackageVersionRange ToPackageVersionRange([NotNull] this NuGet.IVersionSpec version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));

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

        [NotNull]
        public static PackageVersion ToPackageVersion([NotNull] this NuGet.SemanticVersion version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));

            return new PackageVersion(version.Version, version.SpecialVersion);
        }

        [NotNull]
        public static NuGet.SemanticVersion ToSemanticVersion([NotNull] this PackageVersion version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));

            return new NuGet.SemanticVersion(version.Version, version.SpecialVersion);
        }

        [NotNull]
        public static NuGet.VersionSpec ToVersionSpec([NotNull] this PackageVersionRange range)
        {
            if (range == null) throw new ArgumentNullException(nameof(range));

            return new NuGet.VersionSpec()
            {
                MinVersion = range.MinVersion != null ? range.MinVersion.ToSemanticVersion() : null,
                IsMinInclusive = range.IsMinInclusive,
                MaxVersion = range.MaxVersion != null ? range.MaxVersion.ToSemanticVersion() : null,
                IsMaxInclusive = range.IsMaxInclusive
            };
        }

        [NotNull]
        public static NuGet.ManifestFile ToManifestFile([NotNull] this ManifestFile file)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            return new NuGet.ManifestFile()
            {
                Source = file.Source,
                Exclude = file.Exclude,
                Target = file.Target
            };
        }

        [NotNull]
        public static NuGet.ManifestMetadata ToManifestMetadata([NotNull] this ManifestMetadata meta)
        {
            if (meta == null) throw new ArgumentNullException(nameof(meta));

            var nugetMeta = new NuGet.ManifestMetadata()
            {
                Id = meta.Id,
                Authors = meta.Authors,
                Description = meta.Description,
                Copyright = meta.Copyright,
                DevelopmentDependency = meta.DevelopmentDependency,
                Version = meta.Version,
                Owners = meta.Owners,
                IconUrl = meta.IconUrl,
                Language = meta.Language,
                LicenseUrl = meta.LicenseUrl,
                MinClientVersionString = meta.MinClientVersionString,
                ProjectUrl = meta.ProjectUrl,
                ReleaseNotes = meta.ReleaseNotes,
                RequireLicenseAcceptance = meta.RequireLicenseAcceptance,
                Summary = meta.Summary,
                Tags = meta.Tags,
                Title = meta.Title
            };
            if (meta.Dependencies.Count > 0)
            {
                // Copy list of dependencies in the first slot of ManifestDependencySet, and create
                // it if it doesn't exist
                nugetMeta.DependencySets = new List<NuGet.ManifestDependencySet>();
                var dependencySet = new NuGet.ManifestDependencySet();
                nugetMeta.DependencySets.Add(dependencySet);
                foreach (var deps in meta.Dependencies)
                {
                    dependencySet.Dependencies.Add(new NuGet.ManifestDependency() { Id = deps.Id, Version = deps.Version });
                }
            }

            return nugetMeta;
        }
    }
}
