// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using System;
using System.ComponentModel;

namespace SiliconStudio.PackageManager
{
    /// <summary>
    /// Nuget abstraction of SemanticVersion, temporary usage until refactor is complete.
    /// </summary>
    public class NugetSemanticVersion: IComparable, IComparable<NugetSemanticVersion>, IEquatable<NugetSemanticVersion>
    {
        public NugetSemanticVersion(string version)
        {
           SemanticVersion = new NuGet.SemanticVersion(version); 
        }

        public NugetSemanticVersion(Version version)
        {
           SemanticVersion = new NuGet.SemanticVersion(version); 
        }

        internal NugetSemanticVersion(NuGet.SemanticVersion version)
        {
            SemanticVersion = version;
        }

        internal NuGet.SemanticVersion SemanticVersion { get; }

        public string SpecialVersion { get { return SemanticVersion.SpecialVersion; } }

        /// <summary>
        /// Version of current.
        /// </summary>
        public Version Version { get { return SemanticVersion.Version; } }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(obj, null)) return 1;
            NugetSemanticVersion semanticVersion = obj as NugetSemanticVersion;
            if (semanticVersion == null)
            {
                throw new ArgumentException("Expected " + nameof(NugetSemanticVersion), nameof(obj));
            }
            return CompareTo(semanticVersion);
        }

        public int CompareTo(NugetSemanticVersion other)
        {
            if (ReferenceEquals(other, null)) return 1;
            return SemanticVersion.CompareTo(other.SemanticVersion);
        }

        public bool Equals(NugetSemanticVersion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return SemanticVersion.Equals(other.SemanticVersion);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NugetSemanticVersion)obj);
        }

        public override int GetHashCode()
        {
            return SemanticVersion.GetHashCode();
        }

        public static bool operator ==(NugetSemanticVersion version1, NugetSemanticVersion version2)
        {
            if (ReferenceEquals(version1, null))
            {
                return ReferenceEquals(version2, null);
            }
            if (ReferenceEquals(version2, null)) return false;

            return version1.SemanticVersion.Equals(version2.SemanticVersion);
        }

        public static bool operator !=(NugetSemanticVersion version1, NugetSemanticVersion version2)
        {
            return !(version1 == version2);
        }

        public static bool operator <(NugetSemanticVersion version1, NugetSemanticVersion version2)
        {
            if (ReferenceEquals(version1, version2)) return false;
            if (ReferenceEquals(version1, null)) return true;
            if (ReferenceEquals(version2, null)) return false;

            return version1.SemanticVersion < version2.SemanticVersion;
        }

        public static bool operator <=(NugetSemanticVersion version1, NugetSemanticVersion version2)
        {
            return version1 == version2 || version1 < version2;
        }

        public static bool operator >(NugetSemanticVersion version1, NugetSemanticVersion version2)
        {
            return version2 < version1;
        }

        public static bool operator >=(NugetSemanticVersion version1, NugetSemanticVersion version2)
        {
            return version1 == version2 || version1 > version2;
        }

        public override string ToString()
        {
            return SemanticVersion.ToString();
        }
    }
}
