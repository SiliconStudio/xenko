// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Metadata for a <see cref="Package"/> accessible from <see cref="Package.Meta"/>.
    /// </summary>
    [DataContract("PackageMeta")]
    [NonIdentifiable]
    public sealed class PackageMeta
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageMeta"/> class.
        /// </summary>
        public PackageMeta()
        {
            Authors = new List<string>();
            Owners = new List<string>();
            Dependencies = new PackageDependencyCollection();
        }

        /// <summary>
        /// Gets or sets the identifier name of this package.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(10)]
        [DefaultValue(null)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the version of this package.
        /// </summary>
        /// <value>The version.</value>
        [DataMember(20)]
        [DefaultValue(null)]
        public PackageVersion Version { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        [DataMember(30)]
        [DefaultValue(null)]
        public string Title { get; set; }

        /// <summary>
        /// Gets the authors.
        /// </summary>
        /// <value>The authors.</value>
        [DataMember(40)]
        public List<string> Authors { get; private set; }

        /// <summary>
        /// Gets the owners.
        /// </summary>
        /// <value>The owners.</value>
        [DataMember(50)]
        public List<string> Owners { get; private set; }

        /// <summary>
        /// Gets or sets the icon URL.
        /// </summary>
        /// <value>The icon URL.</value>
        [DataMember(60)]
        [DefaultValue(null)]
        public Uri IconUrl { get; set; }

        /// <summary>
        /// Gets or sets the license URL.
        /// </summary>
        /// <value>The license URL.</value>
        [DataMember(70)]
        [DefaultValue(null)]
        public Uri LicenseUrl { get; set; }

        /// <summary>
        /// Gets or sets the project URL.
        /// </summary>
        /// <value>The project URL.</value>
        [DataMember(80)]
        [DefaultValue(null)]
        public Uri ProjectUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether it requires license acceptance.
        /// </summary>
        /// <value><c>true</c> if it requires license acceptance; otherwise, <c>false</c>.</value>
        [DataMember(90)]
        [DefaultValue(false)]
        public bool RequireLicenseAcceptance { get; set; }

        /// <summary>
        /// Gets or sets the description of this package.
        /// </summary>
        /// <value>The description.</value>
        [DataMember(100)]
        [DefaultValue(null)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the summary of this package.
        /// </summary>
        /// <value>The summary.</value>
        [DataMember(110)]
        [DefaultValue(null)]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the release notes of this package.
        /// </summary>
        /// <value>The release notes.</value>
        [DataMember(120)]
        [DefaultValue(null)]
        public string ReleaseNotes { get; set; }

        /// <summary>
        /// Gets or sets the language supported by this package.
        /// </summary>
        /// <value>The language.</value>
        [DataMember(130)]
        [DefaultValue(null)]
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the tags associated to this package.
        /// </summary>
        /// <value>The tags.</value>
        [DataMember(140)]
        [DefaultValue(null)]
        public string Tags { get; set; }

        /// <summary>
        /// Gets or sets the copyright.
        /// </summary>
        /// <value>The copyright.</value>
        [DataMember(150)]
        [DefaultValue(null)]
        public string Copyright { get; set; }

        /// <summary>
        /// Gets or sets the default namespace for this package.
        /// </summary>
        /// <value>The default namespace.</value>
        [DataMember(155)]
        [DefaultValue(null)]
        public string RootNamespace { get; set; }

        /// <summary>
        /// Gets the package dependencies.
        /// </summary>
        /// <value>The package dependencies.</value>
        [DataMember(160)]
        public PackageDependencyCollection Dependencies { get; private set; }

        /// <summary>
        /// Gets the report abuse URL. Only valid for store packages.
        /// </summary>
        /// <value>The report abuse URL.</value>
        [DataMemberIgnore]
        public Uri ReportAbuseUrl { get; private set; }

        /// <summary>
        /// Gets the download count. Only valid for store packages.
        /// </summary>
        /// <value>The download count.</value>
        [DataMemberIgnore]
        public int DownloadCount { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is absolute latest version.
        /// </summary>
        /// <value><c>true</c> if this instance is absolute latest version; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool IsAbsoluteLatestVersion { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is latest version.
        /// </summary>
        /// <value><c>true</c> if this instance is latest version; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool IsLatestVersion { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PackageMeta"/> is listed.
        /// </summary>
        /// <value><c>true</c> if listed; otherwise, <c>false</c>.</value>
        [DataMemberIgnore]
        public bool Listed { get; private set; }

        /// <summary>
        /// Gets the published time.
        /// </summary>
        /// <value>The published.</value>
        [DataMemberIgnore]
        public DateTimeOffset? Published { get; private set; }

        /// <summary>
        /// Copies local and store depdencies of this instance to the specified package
        /// </summary>
        /// <param name="packageMeta">The package meta.</param>
        /// <exception cref="System.ArgumentNullException">packageMeta</exception>
        public void CopyDependenciesTo(PackageMeta packageMeta)
        {
            if (packageMeta == null) throw new ArgumentNullException("packageMeta");
            foreach (var packageDependency in Dependencies)
            {
                if (!packageMeta.Dependencies.Contains(packageDependency))
                {
                    packageMeta.Dependencies.Add(packageDependency.Clone());
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="PackageMeta" /> with default values.
        /// </summary>
        /// <param name="packageName">Name of the package.</param>
        /// <returns>PackageMeta.</returns>
        /// <exception cref="System.ArgumentNullException">packageName</exception>
        public static PackageMeta NewDefault(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName)) throw new ArgumentNullException("packageName");

            var meta = new PackageMeta()
                {
                    Name = packageName,
                    Version = new PackageVersion("1.0.0"),
                    Description = "Modify description of this package here",
                };
            meta.Authors.Add("Modify Author of this package here");

            return meta;
        }

        /// <summary>
        /// Initializes from a nuget package.
        /// </summary>
        /// <param name="metadata">The nuget metadata.</param>
        private void InitializeFrom(NuGet.IPackageMetadata metadata)
        {
            Name = metadata.Id;
            Version = new PackageVersion(metadata.Version.ToString());
            Title = metadata.Title;
            Authors.AddRange(metadata.Authors);
            Owners.AddRange(metadata.Owners);
            IconUrl = metadata.IconUrl;
            LicenseUrl = metadata.LicenseUrl;
            ProjectUrl = metadata.ProjectUrl;
            RequireLicenseAcceptance = metadata.RequireLicenseAcceptance;
            Description = metadata.Description;
            Summary = metadata.Summary;
            ReleaseNotes = metadata.ReleaseNotes;
            Language = metadata.Language;
            Tags = metadata.Tags;
            Copyright = metadata.Copyright;

            var dependencySets = metadata.DependencySets.ToList();
            if (dependencySets.Count > 1)
            {
                throw new InvalidOperationException("Metadata loaded from nuspec cannot have more than one group of dependency");
            }

            // Load dependencies
            Dependencies.Clear();
            var dependencySet = dependencySets.FirstOrDefault();
            if (dependencySet != null)
            {
                foreach (var dependency in dependencySet.Dependencies)
                {
                    var packageDependency = new PackageDependency(dependency.Id, PackageVersionRange.FromVersionSpec(dependency.VersionSpec));
                    Dependencies.Add(packageDependency);
                }
            }

            var serverMetaData = metadata as NuGet.IServerPackageMetadata;
            if (serverMetaData != null)
            {
                ReportAbuseUrl = serverMetaData.ReportAbuseUrl;
                DownloadCount = serverMetaData.DownloadCount;
            }

            var package = metadata as NuGet.IPackage;
            if (package != null)
            {
                IsAbsoluteLatestVersion = package.IsAbsoluteLatestVersion;
                IsLatestVersion = package.IsLatestVersion;
                Listed = package.Listed;
                Published = package.Published;
            }
        }

        public static PackageMeta FromNuGet(NuGet.IPackageMetadata metadata)
        {
            var packageMeta = new PackageMeta();
            packageMeta.InitializeFrom(metadata);
            return packageMeta;
        }

        public NuGet.Manifest ToNugetManifest()
        {
            var manifestMeta = new NuGet.ManifestMetadata();
            ToNugetManifest(manifestMeta);
            return new NuGet.Manifest() { Metadata = manifestMeta };
        }

        public void ToNugetManifest(NuGet.ManifestMetadata manifestMeta)
        {
            manifestMeta.Id = this.Name;
            manifestMeta.Version = this.Version.ToString();
            manifestMeta.Title = this.Title.SafeTrim();
            manifestMeta.Authors = string.Join(",", this.Authors);
            manifestMeta.Owners = string.Join(",", Owners.Count == 0 ? Authors : Owners);
            manifestMeta.Tags = String.IsNullOrEmpty(this.Tags) ? null : this.Tags.SafeTrim();
            manifestMeta.LicenseUrl = ConvertUrlToStringSafe(this.LicenseUrl);
            manifestMeta.ProjectUrl = ConvertUrlToStringSafe(this.ProjectUrl);
            manifestMeta.IconUrl = ConvertUrlToStringSafe(this.IconUrl);
            manifestMeta.RequireLicenseAcceptance = this.RequireLicenseAcceptance;
            manifestMeta.DevelopmentDependency = false;
            manifestMeta.Description = this.Description.SafeTrim();
            manifestMeta.Copyright = this.Copyright.SafeTrim();
            manifestMeta.Summary = this.Summary.SafeTrim();
            manifestMeta.ReleaseNotes = this.ReleaseNotes.SafeTrim();
            manifestMeta.Language = this.Language.SafeTrim();
            manifestMeta.DependencySets = new List<NuGet.ManifestDependencySet>();
            manifestMeta.FrameworkAssemblies = new List<NuGet.ManifestFrameworkAssembly>();
            manifestMeta.ReferenceSets = new List<NuGet.ManifestReferenceSet>();

            var dependencySet = new NuGet.ManifestDependencySet();
            foreach (var dependency in Dependencies)
            {
                if (manifestMeta.DependencySets.Count == 0)
                    manifestMeta.DependencySets.Add(dependencySet);

                dependencySet.Dependencies.Add(new NuGet.ManifestDependency() { Id = dependency.Name, Version = dependency.Version.ToString() });
            }
        }

        private static string ConvertUrlToStringSafe(Uri url)
        {
            if (url != null)
            {
                string originalString = url.OriginalString.SafeTrim();
                if (!string.IsNullOrEmpty(originalString))
                {
                    return originalString;
                }
            }

            return null;
        }
    }
}