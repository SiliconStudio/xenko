// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetVersionSpec
    {
        internal VersionSpec VersionSpec { get; }

        public NugetVersionSpec(NugetSemanticVersion nugetSemanticVersion)
        {
            VersionSpec = new VersionSpec(nugetSemanticVersion.SemanticVersion);
        }

        internal NugetVersionSpec(IVersionSpec spec)
        {
            var concrete = spec as VersionSpec;
            if (concrete != null)
            {
                VersionSpec = concrete;
            }
            else
            {
                VersionSpec = new VersionSpec()
                {
                    MinVersion = spec.MinVersion,
                    MaxVersion = spec.MaxVersion,
                    IsMinInclusive = spec.IsMinInclusive,
                    IsMaxInclusive = spec.IsMaxInclusive
                };
            }
        }

        internal NugetVersionSpec()
        {
            VersionSpec = new VersionSpec();
        }


        protected bool Equals(NugetVersionSpec other)
        {
            return Equals(VersionSpec, other.VersionSpec);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NugetVersionSpec)obj);
        }

        public override int GetHashCode()
        {
            return (VersionSpec != null ? VersionSpec.GetHashCode() : 0);
        }

        public static bool operator ==(NugetVersionSpec left, NugetVersionSpec right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NugetVersionSpec left, NugetVersionSpec right)
        {
            return !Equals(left, right);
        }

        public bool IsMaxInclusive
        {
            get
            {
                return VersionSpec.IsMaxInclusive;
            }
            set
            {
                VersionSpec.IsMaxInclusive = value;
            }
        }

        public bool IsMinInclusive
        {
            get
            {
                return VersionSpec.IsMinInclusive;
            }
            set
            {
                VersionSpec.IsMinInclusive = value;
            }
        }

        public NugetSemanticVersion MaxVersion
        {
            get
            {
                return new NugetSemanticVersion(VersionSpec.MaxVersion);
            }
            set
            {
                VersionSpec.MaxVersion = value?.SemanticVersion;
            }
        }

        public NugetSemanticVersion MinVersion
        {
            get
            {
                return new NugetSemanticVersion(VersionSpec.MinVersion);
            }
            set
            {
                VersionSpec.MinVersion = value?.SemanticVersion;
            }
        }
    }
}
