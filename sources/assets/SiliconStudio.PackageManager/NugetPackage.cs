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

        /// <summary>
        /// Semantic version of current package.
        /// </summary>
        public NugetSemanticVersion Version => new NugetSemanticVersion(_package.Version);

        /// <summary>
        /// Nuget package associated to current.
        /// </summary>
        internal IPackage IPackage => _package;

        public string Id => _package.Id;

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
    }
}
