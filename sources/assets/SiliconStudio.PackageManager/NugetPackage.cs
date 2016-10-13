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
            _packageMetadata = package;
        }

        private readonly NuGet.IPackageMetadata _packageMetadata;

        protected bool Equals(NugetPackage other)
        {
            return Equals(_packageMetadata, other._packageMetadata);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NugetPackage)obj);
        }

        public override int GetHashCode()
        {
            return (_packageMetadata != null ? _packageMetadata.GetHashCode() : 0);
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
        public NugetSemanticVersion Version => new NugetSemanticVersion(_packageMetadata.Version);

        /// <summary>
        /// Nuget IPackage associated to current.
        /// </summary>
        internal NuGet.IPackage IPackage
        {
            get
            {
                var p = _packageMetadata as NuGet.IPackage;
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
                var p = _packageMetadata as NuGet.IServerPackageMetadata;
                return p;
            }
        }

        public string Id => _packageMetadata.Id;

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

        public string Title => _packageMetadata.Title;

        public IEnumerable<string> Authors => _packageMetadata.Authors;

        public IEnumerable<string> Owners => _packageMetadata.Owners;

        public Uri IconUrl => _packageMetadata.IconUrl;

        public Uri LicenseUrl => _packageMetadata.LicenseUrl;

        public Uri ProjectUrl => _packageMetadata.ProjectUrl;

        public bool RequireLicenseAcceptance => _packageMetadata.RequireLicenseAcceptance;

        public bool DevelopmentDependency => _packageMetadata.DevelopmentDependency;

        public string Description => _packageMetadata.Description;

        public string Summary => _packageMetadata.Summary;

        public string ReleaseNotes => _packageMetadata.ReleaseNotes;

        public string Language => _packageMetadata.Language;

        public string Tags => _packageMetadata.Tags;

        public string Copyright => _packageMetadata.Copyright;

        public IEnumerable<NuGet.FrameworkAssemblyReference> FrameworkAssemblies => _packageMetadata.FrameworkAssemblies;

        public ICollection<NuGet.PackageReferenceSet> PackageAssemblyReferences => _packageMetadata.PackageAssemblyReferences;

        public IEnumerable<NuGet.PackageDependencySet> DependencySets => _packageMetadata.DependencySets;

        public Version MinClientVersion => _packageMetadata.MinClientVersion;
        
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
