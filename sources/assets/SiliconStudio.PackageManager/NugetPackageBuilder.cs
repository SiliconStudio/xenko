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
        internal IPackageBuilder Builder { get; }

        protected bool Equals(NugetPackageBuilder other)
        {
            return Equals(Builder, other.Builder);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NugetPackageBuilder)obj);
        }

        public override int GetHashCode()
        {
            return Builder?.GetHashCode() ?? 0;
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

        public IEnumerable<string> Authors => Builder.Authors;

        public string Copyright => Builder.Copyright;

        public IEnumerable<PackageDependencySet> DependencySets => Builder.DependencySets;

        public string Description => Builder.Description;

        public bool DevelopmentDependency => Builder.DevelopmentDependency;

        public Collection<IPackageFile> Files => Builder.Files;

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies => Builder.FrameworkAssemblies;

        public Uri IconUrl => Builder.IconUrl;

        public string Id => Builder.Id;

        public string Language => Builder.Language;

        public Uri LicenseUrl => Builder.LicenseUrl;

        public Version MinClientVersion => Builder.MinClientVersion;

        public IEnumerable<string> Owners => Builder.Owners;

        public ICollection<PackageReferenceSet> PackageAssemblyReferences => Builder.PackageAssemblyReferences;

        public Uri ProjectUrl => Builder.ProjectUrl;

        public string ReleaseNotes => Builder.ReleaseNotes;

        public bool RequireLicenseAcceptance => Builder.RequireLicenseAcceptance;

        public string Summary => Builder.Summary;

        public string Tags => Builder.Tags;

        public string Title => Builder.Title;

        public PackageVersion Version => Builder.Version.ToPackageVersion();

        public void Save(Stream stream)
        {
            Builder.Save(stream);
        }

        public void Populate(ManifestMetadata meta)
        {
            ((PackageBuilder)Builder).Populate(meta.ToManifestMetadata());
        }

        public void PopulateFiles(UDirectory rootDirectory, List<ManifestFile> files)
        {
            ((PackageBuilder)Builder).PopulateFiles(rootDirectory, ToManifsetFiles(files));
        }

        public static IEnumerable<NuGet.ManifestFile> ToManifsetFiles(IEnumerable<ManifestFile> list)
        {
            var res = new List<NuGet.ManifestFile>();
            foreach (var entry in list)
            {
                res.Add(entry.ToManifestFile());
            }
            return res;
        }
    }
}
