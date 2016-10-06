// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NuGet;

namespace SiliconStudio.PackageManager
{
    /// <summary>
    /// Nuget abstraction of a IPackage, temporary usage until refactor is complete.
    /// </summary>
    public class NugetPackage
    {
        internal NugetPackage(IPackage package)
        {
            _package = package;
        }

        private readonly IPackage _package;

        protected bool Equals(NugetPackage other)
        {
            return Equals(_package, other._package);
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
            return (_package != null ? _package.GetHashCode() : 0);
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
        public NugetSemanticVersion Version => new NugetSemanticVersion(_package.Version);

        /// <summary>
        /// Nuget package associated to current.
        /// </summary>
        internal IPackage IPackage => _package;

        public string Id => _package.Id;

        public bool IsAbsoluteLatestVersion => _package.IsAbsoluteLatestVersion;
        public bool IsLatestVersion => _package.IsLatestVersion;
        public bool Listed => _package.Listed;
        public DateTimeOffset? Published => _package.Published;

        public IEnumerable<NugetPackageFile> GetFiles()
        {
            var files = _package.GetFiles();
            var res = new List<NugetPackageFile>();
            foreach (var file in files)
            {
                res.Add(new NugetPackageFile(file));
            }
            return res;
        }

        public string Title => _package.Title;

        public IEnumerable<string> Authors => _package.Authors;

        public IEnumerable<string> Owners => _package.Owners;

        public Uri IconUrl => _package.IconUrl;

        public Uri LicenseUrl => _package.LicenseUrl;

        public Uri ProjectUrl => _package.ProjectUrl;

        public bool RequireLicenseAcceptance => _package.RequireLicenseAcceptance;

        public bool DevelopmentDependency => _package.DevelopmentDependency;

        public string Description => _package.Description;

        public string Summary => _package.Summary;

        public string ReleaseNotes => _package.ReleaseNotes;

        public string Language => _package.Language;

        public string Tags => _package.Tags;

        public string Copyright => _package.Copyright;

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies => _package.FrameworkAssemblies;

        public ICollection<PackageReferenceSet> PackageAssemblyReferences => _package.PackageAssemblyReferences;

        public IEnumerable<PackageDependencySet> DependencySets => _package.DependencySets;

        public Version MinClientVersion => _package.MinClientVersion;
        
        public int DownloadCount => _package.DownloadCount;

        public Uri ReportAbuseUrl => _package.ReportAbuseUrl;

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
                        res.Add(new Tuple<string, PackageVersionRange>(dependency.Id, PackageVersionRange.FromVersionSpec(new NugetVersionSpec(dependency.VersionSpec))));
                    }
                }
                return res;
            }
        }

    }
}
