// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using System;
using System.Collections;
using System.Collections.Generic;
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

        public string Title
        {
            get { return _package.Title; }
        }

        public IEnumerable<string> Authors
        {
            get { return _package.Authors; }
        }

        public IEnumerable<string> Owners
        {
            get { return _package.Owners; }
        }

        public Uri IconUrl
        {
            get { return _package.IconUrl; }
        }

        public Uri LicenseUrl
        {
            get { return _package.LicenseUrl; }
        }

        public Uri ProjectUrl
        {
            get { return _package.ProjectUrl; }
        }

        public bool RequireLicenseAcceptance
        {
            get { return _package.RequireLicenseAcceptance; }
        }

        public bool DevelopmentDependency
        {
            get { return _package.DevelopmentDependency; }
        }

        public string Description
        {
            get { return _package.Description; }
        }

        public string Summary
        {
            get { return _package.Summary; }
        }

        public string ReleaseNotes
        {
            get { return _package.ReleaseNotes; }
        }

        public string Language
        {
            get { return _package.Language; }
        }

        public string Tags
        {
            get { return _package.Tags; }
        }

        public string Copyright
        {
            get { return _package.Copyright; }
        }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies
        {
            get { return _package.FrameworkAssemblies; }
        }

        public ICollection<PackageReferenceSet> PackageAssemblyReferences
        {
            get { return _package.PackageAssemblyReferences; }
        }

        public IEnumerable<PackageDependencySet> DependencySets
        {
            get { return _package.DependencySets; }
        }

        public Version MinClientVersion
        {
            get { return _package.MinClientVersion; }
        }

        public object DownloadCount
        {
            get { return _package.DownloadCount; } 
        }
    }
}
