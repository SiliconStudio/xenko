// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetManifestMetadata
    {
        internal NugetManifestMetadata(ManifestMetadata metadata)
        {
            Metadata = metadata;
            DependencySets = new List<ManifestDependencySet>();
            ReferenceSets = new List<ManifestReferenceSet>();
            FrameworkAssemblies = new List<ManifestFrameworkAssembly>();
        }

        protected bool Equals(NugetManifestMetadata other)
        {
            return Equals(Metadata, other.Metadata);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NugetManifestMetadata)obj);
        }

        public override int GetHashCode()
        {
            return Metadata?.GetHashCode() ?? 0;
        }

        public static bool operator ==(NugetManifestMetadata left, NugetManifestMetadata right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NugetManifestMetadata left, NugetManifestMetadata right)
        {
            return !Equals(left, right);
        }

        public NugetManifestMetadata()
        {
            Metadata = new ManifestMetadata();
        }

        public ManifestMetadata Metadata { get; }

        public string MinClientVersionString { get { return Metadata.MinClientVersionString; } set { Metadata.MinClientVersionString = value; } }
        public string Id { get { return Metadata.Id; } set { Metadata.Id = value; } }
        public string Version { get { return Metadata.Version; } set { Metadata.Version = value; } }
        public string Title { get { return Metadata.Title; } set { Metadata.Title = value; } }
        public string Authors { get { return Metadata.Authors; } set { Metadata.Authors = value; } }
        public string Owners { get { return Metadata.Owners; } set { Metadata.Owners = value; } }
        public string LicenseUrl { get { return Metadata.LicenseUrl; } set { Metadata.LicenseUrl = value; } }
        public string ProjectUrl { get { return Metadata.ProjectUrl; } set { Metadata.ProjectUrl = value; } }
        public string IconUrl { get { return Metadata.IconUrl; } set { Metadata.IconUrl = value; } }
        public bool RequireLicenseAcceptance { get { return Metadata.RequireLicenseAcceptance; } set { Metadata.RequireLicenseAcceptance = value; } }
        public bool DevelopmentDependency { get { return Metadata.DevelopmentDependency; } set { Metadata.DevelopmentDependency = value; } }
        public string Description { get { return Metadata.Description; } set { Metadata.Description = value; } }
        public string Summary { get { return Metadata.Summary; } set { Metadata.Summary = value; } }
        public string ReleaseNotes { get { return Metadata.ReleaseNotes; } set { Metadata.ReleaseNotes = value; } }
        public string Copyright { get { return Metadata.Copyright; } set { Metadata.Copyright = value; } }
        public string Language { get { return Metadata.Language; } set { Metadata.Language = value; } }
        public string Tags { get { return Metadata.Tags; } set { Metadata.Tags = value; } }
        public List<object> DependencySetsSerialize { get { return Metadata.DependencySetsSerialize; } set { Metadata.DependencySetsSerialize = value; } }
        public List<ManifestDependencySet> DependencySets
        {
            get { return Metadata.DependencySets; }
            private set { Metadata.DependencySets = value; }
        }

        public List<ManifestFrameworkAssembly> FrameworkAssemblies
        {
            get { return Metadata.FrameworkAssemblies; }
            private set { Metadata.FrameworkAssemblies = value; }
        }

        public List<ManifestReferenceSet> ReferenceSets
        {
            get { return Metadata.ReferenceSets; }
            private set { Metadata.ReferenceSets = value; }
        }

        /// <summary>
        /// Add new dependency to package name <paramref name="name"/> with version <paramref name="v"/> to
        /// the first set if it exists already, otherwise create a new sets where dependency will be added to.
        /// </summary>
        /// <param name="name">Name of package to add to <see cref="DependencySets"/></param>
        /// <param name="v">Version of package to add to <see cref="DependencySets"/></param>
        public void AddDependency(string name, PackageVersionRange v)
        {
            ManifestDependencySet dependencySet;
            if (DependencySets.Count == 0)
            {
                dependencySet = new ManifestDependencySet();
                DependencySets.Add(dependencySet);
            }
            else
            {
                dependencySet = DependencySets[0];
            }

            dependencySet.Dependencies.Add(new ManifestDependency() { Id = name, Version = v.ToString() });
        }
    }
}
