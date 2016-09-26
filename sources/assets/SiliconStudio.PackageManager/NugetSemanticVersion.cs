// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using System;

namespace SiliconStudio.PackageManager
{
    /// <summary>
    /// Nuget abstraction of SemanticVersion, temporary usage until refactor is complete.
    /// </summary>
    public class NugetSemanticVersion 
    {
        public NugetSemanticVersion(string version)
        {
           _version = new NuGet.SemanticVersion(version); 
        }

        public NugetSemanticVersion(Version version)
        {
           _version = new NuGet.SemanticVersion(version); 
        }

        internal NugetSemanticVersion(NuGet.SemanticVersion version)
        {
            _version = version;
        }

        private NuGet.SemanticVersion _version;

        /// <summary>
        /// Version of current.
        /// </summary>
        public Version Version { get { return _version.Version; } }
    }
}
