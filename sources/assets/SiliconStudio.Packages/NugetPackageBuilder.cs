// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using NuGet;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Abstraction to build a NuGet package.
    /// </summary>
    public class NugetPackageBuilder
    {
        internal IPackageBuilder Builder { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="NugetPackageBuilder"/>.
        /// </summary>
        public NugetPackageBuilder()
        {
            Builder = new PackageBuilder();
        }

        /// <summary>
        /// Determines whether the <paramref name="other"/> object is equal to the current object.
        /// </summary>
        /// <param name="other">The object to compare against the current object.</param>
        /// <returns><c>true</c> if <paramref name="other"/> is equal to the current object, <c>false</c> otherwise.</returns>
        protected bool Equals(NugetPackageBuilder other)
        {
            return Equals(Builder, other.Builder);
        }

        /// <inheritdoc cref="object"/>
        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != GetType()) return false;
            return Equals((NugetPackageBuilder)other);
        }

        /// <inheritdoc cref="object"/>
        public override int GetHashCode()
        {
            return Builder?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Determines whether two specified <see cref="NugetPackageBuilder"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="NugetPackageBuilder"/>object.</param>
        /// <param name="right">The second <see cref="NugetPackageBuilder"/>object.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is equal to <paramref name="right"/>, <c>false</c> otherwise.</returns>
        public static bool operator ==(NugetPackageBuilder left, NugetPackageBuilder right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Determines whether two specified <see cref="NugetPackageBuilder"/> objects are equal.
        /// </summary>
        /// <param name="left">The first <see cref="NugetPackageBuilder"/>object.</param>
        /// <param name="right">The second <see cref="NugetPackageBuilder"/>object.</param>
        /// <returns><c>true</c> if <paramref name="left"/> is not equal to <paramref name="right"/>, <c>false</c> otherwise.</returns>
        public static bool operator !=(NugetPackageBuilder left, NugetPackageBuilder right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// The authors of this new package.
        /// </summary>
        public IEnumerable<string> Authors => Builder.Authors;

        /// <summary>
        /// The copyright of this new package.
        /// </summary>
        public string Copyright => Builder.Copyright;

        public IEnumerable<PackageDependencySet> DependencySets => Builder.DependencySets;

        /// <summary>
        /// The description of this new package.
        /// </summary>
        public string Description => Builder.Description;

        /// <summary>
        /// Determines if this new package is used for development purpose and should not be listed as a dependency.
        /// </summary>
        public bool DevelopmentDependency => Builder.DevelopmentDependency;

        public Collection<IPackageFile> Files => Builder.Files;

        public IEnumerable<FrameworkAssemblyReference> FrameworkAssemblies => Builder.FrameworkAssemblies;

        /// <summary>
        /// The URL of this new package's icon.
        /// </summary>
        public Uri IconUrl => Builder.IconUrl;

        /// <summary>
        /// The Id of this new package.
        /// </summary>
        public string Id => Builder.Id;

        /// <summary>
        /// The language of this new package.
        /// </summary>
        public string Language => Builder.Language;

        /// <summary>
        /// The URL of this new package's license.
        /// </summary>
        public Uri LicenseUrl => Builder.LicenseUrl;

        /// <summary>
        /// The minimum client supported by this new package.
        /// </summary>
        public Version MinClientVersion => Builder.MinClientVersion;

        /// <summary>
        /// The owners of this new package.
        /// </summary>
        public IEnumerable<string> Owners => Builder.Owners;

        public ICollection<PackageReferenceSet> PackageAssemblyReferences => Builder.PackageAssemblyReferences;

        /// <summary>
        /// The URL of this new package's project.
        /// </summary>
        public Uri ProjectUrl => Builder.ProjectUrl;

        /// <summary>
        /// The release notes of this new package.
        /// </summary>
        public string ReleaseNotes => Builder.ReleaseNotes;

        /// <summary>
        /// Determines if this new package requires a license acceptance.
        /// </summary>
        public bool RequireLicenseAcceptance => Builder.RequireLicenseAcceptance;

        /// <summary>
        /// The summary description of this new package.
        /// </summary>
        public string Summary => Builder.Summary;

        /// <summary>
        /// The list of tags of this package separated by spaces.
        /// </summary>
        public string Tags => Builder.Tags;

        /// <summary>
        /// The title of this new package.
        /// </summary>
        public string Title => Builder.Title;

        public PackageVersion Version => Builder.Version.ToPackageVersion();

        /// <summary>
        /// Saves this new package in <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The stream where package will be saved.</param>
        public void Save(Stream stream)
        {
            Builder.Save(stream);
        }

        /// <summary>
        /// Fills the builder with the manifest metadata containing all the information about this new package.
        /// </summary>
        /// <param name="meta">The manifest metadata.</param>
        public void Populate(ManifestMetadata meta)
        {
            ((PackageBuilder)Builder).Populate(meta.ToManifestMetadata());
        }

        /// <summary>
        /// Fills the builder with the list of files that are part of this new package.
        /// </summary>
        /// <param name="rootDirectory">The root location where files are located.</param>
        /// <param name="files">The files to include to the builder.</param>
        public void PopulateFiles(UDirectory rootDirectory, List<ManifestFile> files)
        {
            ((PackageBuilder)Builder).PopulateFiles(rootDirectory, ToManifestFiles(files));
        }

        /// <summary>
        /// Converts a list of <see cref="ManifestFile"/> into a list of <see cref="NuGet.ManifestFile"/>.
        /// </summary>
        /// <param name="list">The list to convert.</param>
        /// <returns>A new list of <see cref="NuGet.ManifestFile"/></returns>
        [NotNull]
        public static IEnumerable<NuGet.ManifestFile> ToManifestFiles(IEnumerable<ManifestFile> list)
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
