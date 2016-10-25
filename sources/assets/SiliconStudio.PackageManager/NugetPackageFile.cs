// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetPackageFile
    {
        private readonly IPackageFile file;

        internal NugetPackageFile(IPackageFile file)
        {
            this.file = file;
        }

        protected bool Equals(NugetPackageFile other)
        {
            return Equals(file, other.file);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NugetPackageFile)obj);
        }

        public override int GetHashCode()
        {
            return file?.GetHashCode() ?? 0;
        }

        public static bool operator ==(NugetPackageFile left, NugetPackageFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NugetPackageFile left, NugetPackageFile right)
        {
            return !Equals(left, right);
        }

        public string Path => file.Path;

        public Stream GetStream()
        {
            return file.GetStream();
        }
    }
}
