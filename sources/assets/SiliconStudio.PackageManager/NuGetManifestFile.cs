// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetManifestFile
    {
        internal ManifestFile File { get; }

        protected bool Equals(NugetManifestFile other)
        {
            return Equals(File, other.File);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NugetManifestFile)obj);
        }

        public override int GetHashCode()
        {
            return (File != null ? File.GetHashCode() : 0);
        }

        public static bool operator ==(NugetManifestFile left, NugetManifestFile right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NugetManifestFile left, NugetManifestFile right)
        {
            return !Equals(left, right);
        }

        internal NugetManifestFile(ManifestFile file)
        {
            File = file;
        }

        public NugetManifestFile()
        {
        }

        public string Source
        {
            get { return File.Source; }
            set { File.Source = value; }
        }

        public string Target
        {
            get { return File.Target; }
            set { File.Target = value; }
        }

        public string Exclude
        {
            get { return File.Exclude; }
            set { File.Exclude = value; }
        }
    }
}