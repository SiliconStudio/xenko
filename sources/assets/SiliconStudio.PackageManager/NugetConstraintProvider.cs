// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetConstraintProvider
    {
        internal DefaultConstraintProvider Provider { get; }

        protected bool Equals(NugetConstraintProvider other)
        {
            return Equals(Provider, other.Provider);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NugetConstraintProvider)obj);
        }

        public override int GetHashCode()
        {
            return (Provider != null ? Provider.GetHashCode() : 0);
        }

        public static bool operator ==(NugetConstraintProvider left, NugetConstraintProvider right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NugetConstraintProvider left, NugetConstraintProvider right)
        {
            return !Equals(left, right);
        }

        public NugetConstraintProvider()
        {
            Provider = new DefaultConstraintProvider();
        }

        public string Source
        {
            get
            {
                return Provider.Source;
            }
        }

        public IVersionSpec GetConstraint(string packageId)
        {
            return Provider.GetConstraint(packageId);
        }

        public void AddConstraint(string name, NugetVersionSpec nugetVersionSpec)
        {
           Provider.AddConstraint(name, nugetVersionSpec.VersionSpec); 
        }
    }
}
