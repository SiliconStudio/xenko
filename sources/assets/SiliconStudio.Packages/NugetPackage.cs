// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Nuget abstraction of a package.
    /// </summary>
    public class NugetPackage
    {
        /// <summary>
        /// Initializes a new instance of <see cref="NugetPackage"/> using some NuGet data.
        /// </summary>
        /// <param name="package">The NuGet metadata we will use to construct the current instance.</param>
        internal NugetPackage(NuGet.IPackageMetadata package)
        {
            packageMetadata = package;
        }

        /// <summary>
        /// Storage for the NuGet metatadata.
        /// </summary>
        private readonly NuGet.IPackageMetadata packageMetadata;

        /// <summary>
        /// Determines whether the <paramref name="other"/> object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare against the current object.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to the current object, <c>false</c> otherwise.</returns>
        protected bool Equals(NugetPackage other)
        {
            return Equals(packageMetadata, other.packageMetadata);
        }

        /// <inheritdoc cref="object"/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NugetPackage)obj);
        }

        /// <inheritdoc cref="object"/>
        public override int GetHashCode()
        {
            return packageMetadata?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Determines whether two specified <see cref="NugetPackage"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="NugetPackage"/>object.</param>
        /// <param name="right">The second <see cref="NugetPackage"/>object.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is equal to <paramref name="right"/>, <c>false</c> otherwise.</returns>
        public static bool operator ==(NugetPackage left, NugetPackage right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines whether two specified <see cref="NugetPackage"/> objects are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="NugetPackage"/>object.</param>
        /// <param name="right">The second <see cref="NugetPackage"/>object.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is not equal to <paramref name="right"/>, <c>false</c> otherwise.</returns>
        public static bool operator !=(NugetPackage left, NugetPackage right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Version of current package.
        /// </summary>
        public PackageVersion Version => packageMetadata.Version.ToPackageVersion();

        /// <summary>
        /// Nuget IPackage associated to current.
        /// </summary>
        /// <remarks>Internal since it exposes a NuGet type.</remarks>
        internal NuGet.IPackage IPackage
        {
            get
            {
                var p = packageMetadata as NuGet.IPackage;
                return p;
            }
        }

        /// <summary>
        /// Nuget IPackage associated to current.
        /// </summary>
        /// <remarks>Internal since it exposes a NuGet type.</remarks>
        internal NuGet.IServerPackageMetadata IServerPackageMetadata
        {
            get
            {
                var p = packageMetadata as NuGet.IServerPackageMetadata;
                return p;
            }
        }

        /// <summary>
        /// The Id of this package.
        /// </summary>
        public string Id => packageMetadata.Id;

        /// <summary>
        /// The listed status of this package.
        /// </summary>
        public bool Listed => IPackage?.Listed ?? false;

        /// <summary>
        /// The date of publication if present.
        /// </summary>
        public DateTimeOffset? Published => IPackage?.Published;

        public IEnumerable<PackageFile> GetFiles([NotNull] string root)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            var res = new List<PackageFile>();
            var files = IPackage?.GetFiles();
            if (files != null)
            {
                foreach (var file in files)
                {
                    // TODO: Verify when testing self-update that `root` + `file.Path` gives us the right path
                    res.Add(new PackageFile(root, file.Path));
                }
            }
            return res;
        }

        /// <summary>
        /// The title of this package.
        /// </summary>
        public string Title => packageMetadata.Title;

        /// <summary>
        /// The list of authors of this package.
        /// </summary>
        public IEnumerable<string> Authors => packageMetadata.Authors;

        /// <summary>
        /// The list of owners of this package.
        /// </summary>
        public IEnumerable<string> Owners => packageMetadata.Owners;

        /// <summary>
        /// The URL of this package's icon.
        /// </summary>
        public Uri IconUrl => packageMetadata.IconUrl;

        /// <summary>
        /// The URL of this package's license.
        /// </summary>
        public Uri LicenseUrl => packageMetadata.LicenseUrl;

        /// <summary>
        /// The URL of this package's project.
        /// </summary>
        public Uri ProjectUrl => packageMetadata.ProjectUrl;

        /// <summary>
        /// Determines if this package requires a license acceptance.
        /// </summary>
        public bool RequireLicenseAcceptance => packageMetadata.RequireLicenseAcceptance;

        /// <summary>
        /// The description of this package.
        /// </summary>
        public string Description => packageMetadata.Description;

        /// <summary>
        /// The summary description of this package.
        /// </summary>
        public string Summary => packageMetadata.Summary;

        public string ReleaseNotes => packageMetadata.ReleaseNotes;

        public string Language => packageMetadata.Language;

        /// <summary>
        /// The list of tags of this package separated by spaces.
        /// </summary>
        public string Tags => packageMetadata.Tags;

        public string Copyright => packageMetadata.Copyright;

        /// <remarks>Internal since it exposes a NuGet type.</remarks>
        internal IEnumerable<NuGet.FrameworkAssemblyReference> FrameworkAssemblies => packageMetadata.FrameworkAssemblies;

        /// <remarks>Internal since it exposes a NuGet type.</remarks>
        internal ICollection<NuGet.PackageReferenceSet> PackageAssemblyReferences => packageMetadata.PackageAssemblyReferences;

        /// <summary>
        /// The list of dependencies of this package.
        /// </summary>
        /// <remarks>Internal since it exposes a NuGet type.</remarks>
        internal IEnumerable<NuGet.PackageDependencySet> DependencySets => packageMetadata.DependencySets;

        public Version MinClientVersion => packageMetadata.MinClientVersion;
        
        /// <summary>
        /// The number of downloads for this package. It is specific to the version of this package.
        /// </summary>
        public long DownloadCount => IServerPackageMetadata?.DownloadCount ?? 0;

        /// <summary>
        /// The URL to report abused on this package.
        /// </summary>
        public Uri ReportAbuseUrl => IServerPackageMetadata?.ReportAbuseUrl;

        /// <summary>
        /// The number of dependency sets.
        /// </summary>
        public int DependencySetsCount => DependencySets.Count();

        /// <summary>
        /// Computed the list of dependencies of this package.
        /// </summary>
        public IEnumerable<Tuple<string, PackageVersionRange>>  Dependencies
        {
            get
            {
                var res = new List<Tuple<string, PackageVersionRange>>();
                var set = DependencySets.FirstOrDefault();
                if (set != null)
                {
                    foreach (var dependency in set.Dependencies)
                    {
                        res.Add(new Tuple<string, PackageVersionRange>(dependency.Id, dependency.VersionSpec.ToPackageVersionRange()));
                    }
                }
                return res;
            }
        }

    }
}
