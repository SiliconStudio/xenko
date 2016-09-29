// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetManifestMetadata
    {
        internal NugetManifestMetadata(ManifestMetadata metadata)
        {
            Metadata = metadata;
        }

        protected bool Equals(NugetManifestMetadata other)
        {
            return Equals(Metadata, other.Metadata);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NugetManifestMetadata)obj);
        }

        public override int GetHashCode()
        {
            return (Metadata != null ? Metadata.GetHashCode() : 0);
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

        public ManifestMetadata Metadata { get; set; }

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
        public List<ManifestDependencySet> DependencySets { get { return Metadata.DependencySets; } set { Metadata.DependencySets = value; } }
        public List<ManifestFrameworkAssembly> FrameworkAssemblies { get { return Metadata.FrameworkAssemblies; } set { Metadata.FrameworkAssemblies = value; } }
        public List<object> ReferenceSetsSerialize { get { return Metadata.ReferenceSetsSerialize; } set { Metadata.ReferenceSetsSerialize = value; } }
        public List<ManifestReferenceSet> ReferenceSets { get { return Metadata.ReferenceSets; } set { Metadata.ReferenceSets = value; } }
        public List<object> ContentFilesSerialize { get { return Metadata.ContentFilesSerialize; } set { Metadata.ContentFilesSerialize = value; } }
        public List<ManifestContentFiles> ContentFiles { get { return Metadata.ContentFiles; } set { Metadata.ContentFiles = value; } }
    }
}
