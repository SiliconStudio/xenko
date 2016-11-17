// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.IO;

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Representation of a file in a package.
    /// </summary>
    public class PackageFile
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PackageFile"/> located in <paramref name="path"/>.
        /// </summary>
        /// <param name="root">Path to the root of the package.</param>
        /// <param name="path">Path of the file in the package.</param>
        public PackageFile(string root, string path)
        {
            Path = path;
        }

        /// <summary>
        /// Gets the full path of the file inside the package.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Access to the stream content in read mode.
        /// </summary>
        /// <returns>A new stream reading file pointed by <see cref="Path"/>.</returns>
        public Stream GetStream()
        {
            return File.OpenRead(Path);
        }
    }
}
