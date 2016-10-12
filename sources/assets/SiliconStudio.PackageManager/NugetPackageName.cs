// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetPackageName
    {
        internal PackageName Name {get;}

        protected bool Equals(NugetPackageName other)
        {
            return Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NugetPackageName)obj);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }

        public static bool operator ==(NugetPackageName left, NugetPackageName right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NugetPackageName left, NugetPackageName right)
        {
            return !Equals(left, right);
        }

        public NugetPackageName(string id, NugetSemanticVersion version)
        {
            Name = new PackageName(id, version.SemanticVersion);
        }

        internal NugetPackageName(PackageName name)
        {
            Name = name;
        }

        public NugetSemanticVersion Version => new NugetSemanticVersion(Name.Version);
    }
}
