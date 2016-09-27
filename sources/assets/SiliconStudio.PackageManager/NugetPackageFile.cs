// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using NuGet;

namespace SiliconStudio.PackageManager
{
    public class NugetPackageFile
    {
        private IPackageFile _file;

        internal NugetPackageFile(IPackageFile file)
        {
            _file = file;
        }

        public string Path => _file.Path;

        public Stream GetStream()
        {
            return _file.GetStream();
        }
    }
}
