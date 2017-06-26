// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
        /// Gets the source path of the file on the hard drive (if it has a source).
        /// </summary>
        public string SourcePath => (packageFile as PhysicalPackageFile)?.SourcePath;

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
