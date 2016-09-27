// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetPackageName
    {
        internal PackageName Name {get;}

        public NugetPackageName(string id, NugetSemanticVersion version)
        {
            Name = new PackageName(id, version.SemanticVersion);
        }

        public NugetSemanticVersion Version => new NugetSemanticVersion(Name.Version);
    }
}
