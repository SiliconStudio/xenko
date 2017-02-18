// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;
using NuGet;

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Representation of a file in a package.
    /// </summary>
    public class PackageFile
    {
        private readonly IPackageFile packageFile;

        /// <summary>
        /// Initializes a new instance of <see cref="PackageFile"/> located in <paramref name="path"/>.
        /// </summary>
        /// <param name="file">Nuget package file</param>
        public PackageFile(IPackageFile file)
        {
            packageFile = file;
        }

        /// <summary>
        /// Gets the full path of the file inside the package.
        /// </summary>
        public string Path => packageFile.Path;

        /// <summary>
        /// Access to the stream content in read mode.
        /// </summary>
        /// <returns>A new stream reading file pointed by <see cref="Path"/>.</returns>
        public Stream GetStream()
        {
            return packageFile.GetStream();
        }
    }
}
