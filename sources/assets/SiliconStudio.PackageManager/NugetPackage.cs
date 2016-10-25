// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.PackageManager
{
    /// <summary>
    /// Nuget abstraction of a IPackage, temporary usage until refactor is complete.
    /// </summary>
    public class NugetPackage
    {
        internal NugetPackage(NuGet.IPackageMetadata package)
        {
            packageMetadata = package;
        }

        private readonly NuGet.IPackageMetadata packageMetadata;

        protected bool Equals(NugetPackage other)
        {
            return Equals(packageMetadata, other.packageMetadata);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NugetPackage)obj);
        }

        public override int GetHashCode()
        {
            return packageMetadata?.GetHashCode() ?? 0;
        }

        public static bool operator ==(NugetPackage left, NugetPackage right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NugetPackage left, NugetPackage right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Semantic version of current package.
        /// </summary>
        public NugetSemanticVersion Version => new NugetSemanticVersion(packageMetadata.Version);

        /// <summary>
        /// Nuget IPackage associated to current.
        /// </summary>
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
        internal NuGet.IServerPackageMetadata IServerPackageMetadata
        {
            get
            {
                var p = packageMetadata as NuGet.IServerPackageMetadata;
                return p;
            }
        }

        public string Id => packageMetadata.Id;

        public bool IsAbsoluteLatestVersion => IPackage?.IsAbsoluteLatestVersion ?? false;

        public bool IsLatestVersion => IPackage?.IsLatestVersion ?? false;
        public bool Listed => IPackage?.Listed ?? false;
        public DateTimeOffset? Published => IPackage?.Published;

        public IEnumerable<NugetPackageFile> GetFiles()
        {
            var res = new List<NugetPackageFile>();
            var files = IPackage?.GetFiles();
            if (files != null)
            {
                foreach (var file in files)
                {
                    res.Add(new NugetPackageFile(file));
                }
            }
            return res;
        }

        public string Title => packageMetadata.Title;

        public IEnumerable<string> Authors => packageMetadata.Authors;

        public IEnumerable<string> Owners => packageMetadata.Owners;

        public Uri IconUrl => packageMetadata.IconUrl;

        public Uri LicenseUrl => packageMetadata.LicenseUrl;

        public Uri ProjectUrl => packageMetadata.ProjectUrl;

        public bool RequireLicenseAcceptance => packageMetadata.RequireLicenseAcceptance;

        public bool DevelopmentDependency => packageMetadata.DevelopmentDependency;

        public string Description => packageMetadata.Description;

        public string Summary => packageMetadata.Summary;

        public string ReleaseNotes => packageMetadata.ReleaseNotes;

        public string Language => packageMetadata.Language;

        public string Tags => packageMetadata.Tags;

        public string Copyright => packageMetadata.Copyright;

        public IEnumerable<NuGet.FrameworkAssemblyReference> FrameworkAssemblies => packageMetadata.FrameworkAssemblies;

        public ICollection<NuGet.PackageReferenceSet> PackageAssemblyReferences => packageMetadata.PackageAssemblyReferences;

        public IEnumerable<NuGet.PackageDependencySet> DependencySets => packageMetadata.DependencySets;

        public Version MinClientVersion => packageMetadata.MinClientVersion;
        
        public int DownloadCount => IServerPackageMetadata?.DownloadCount ?? 0;

        public Uri ReportAbuseUrl => IServerPackageMetadata?.ReportAbuseUrl;

        public int DependencySetsCount => DependencySets.Count();

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
