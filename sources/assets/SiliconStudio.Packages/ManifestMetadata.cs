// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Packages
{
    public class ManifestMetadata
    {
        public ManifestMetadata()
        {
            Dependencies = new List<ManifestDependency>();
        }

        public string MinClientVersionString { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public string Owners { get; set; }
        public string LicenseUrl { get; set; }
        public string ProjectUrl { get; set; }
        public string IconUrl { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public bool DevelopmentDependency { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public string ReleaseNotes { get; set; }
        public string Copyright { get; set; }
        public string Language { get; set; }
        public string Tags { get; set; }
        public List<ManifestDependency> Dependencies { get; set; }

        /// <summary>
        /// Add new dependency to package name <paramref name="name"/> with version <paramref name="v"/> to
        /// the first set if it exists already, otherwise create a new sets where dependency will be added to.
        /// </summary>
        /// <param name="name">Name of package to add to <see cref="Dependencies"/></param>
        /// <param name="v">Version range accepted for package to add to <see cref="Dependencies"/></param>
        public void AddDependency(string name, PackageVersionRange v)
        {
            Dependencies.Add(new ManifestDependency() { Id = name, Version = v.ToString() });
        }
    }
}
