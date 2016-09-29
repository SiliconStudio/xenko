// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using NuGet;
using SiliconStudio.Core.IO;

namespace SiliconStudio.PackageManager
{
    public class NugetPackageBuilder
    {
        internal IPackageBuilder Builder { get; private set; }

        protected bool Equals(NugetPackageBuilder other)
        {
            return Equals(Builder, other.Builder);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NugetPackageBuilder)obj);
        }

        public override int GetHashCode()
        {
            return (Builder != null ? Builder.GetHashCode() : 0);
        }

        public static bool operator ==(NugetPackageBuilder left, NugetPackageBuilder right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NugetPackageBuilder left, NugetPackageBuilder right)
        {
            return !Equals(left, right);
        }

        public NugetPackageBuilder(PackageBuilder builder)
        {
            Builder = builder;
        }

        public NugetPackageBuilder()
        {
            Builder = new PackageBuilder();
        }

        public IEnumerable<string> Authors
        {
            get
            {
                return Builder.Authors;
            }
        }

        public string Copyright
        {
            get
            {
                return Builder.Copyright;
            }
        }

        public IEnumerable<PackageDependencySet> DependencySets
        {
            get
            {
                return Builder.DependencySets;
            }
        }

        public string Description
        {
            get
            {
                return Builder.Description;
            }
        }

        public bool DevelopmentDependency
        {
            get
            {
                return Builder.DevelopmentDependency;
            }
        }

        public Collection<IPackageFile> Files
        {
            get
            {
                return Builder.Files;
            }
        }

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies
        {
            get
            {
                return Builder.FrameworkAssemblies;
            }
        }

        public Uri IconUrl
        {
            get
            {
                return Builder.IconUrl;
            }
        }

        public string Id
        {
            get
            {
                return Builder.Id;
            }
        }

        public string Language
        {
            get
            {
                return Builder.Language;
            }
        }

        public Uri LicenseUrl
        {
            get
            {
                return Builder.LicenseUrl;
            }
        }

        public Version MinClientVersion
        {
            get
            {
                return Builder.MinClientVersion;
            }
        }

        public IEnumerable<string> Owners
        {
            get
            {
                return Builder.Owners;
            }
        }

        public ICollection<PackageReferenceSet> PackageAssemblyReferences
        {
            get
            {
                return Builder.PackageAssemblyReferences;
            }
        }

        public Uri ProjectUrl
        {
            get
            {
                return Builder.ProjectUrl;
            }
        }

        public string ReleaseNotes
        {
            get
            {
                return Builder.ReleaseNotes;
            }
        }

        public bool RequireLicenseAcceptance
        {
            get
            {
                return Builder.RequireLicenseAcceptance;
            }
        }

        public string Summary
        {
            get
            {
                return Builder.Summary;
            }
        }

        public string Tags
        {
            get
            {
                return Builder.Tags;
            }
        }

        public string Title
        {
            get
            {
                return Builder.Title;
            }
        }

        public NugetSemanticVersion Version
        {
            get
            {
                return new NugetSemanticVersion(Builder.Version);
            }
        }

        public void Save(Stream stream)
        {
            Builder.Save(stream);
        }

        public void Populate(NugetManifestMetadata meta)
        {
            ((PackageBuilder)Builder).Populate(meta.Metadata);
        }

        public void PopulateFiles(UDirectory rootDirectory, List<NugetManifestFile> files)
        {
            ((PackageBuilder)Builder).PopulateFiles(rootDirectory, ToManifsetFiles(files));
        }

        public static IEnumerable<ManifestFile> ToManifsetFiles(IEnumerable<NugetManifestFile> list)
        {
            var res = new List<ManifestFile>();
            foreach (var entry in list)
            {
                res.Add(entry.File);
            }
            return res;
        }
    }
}
