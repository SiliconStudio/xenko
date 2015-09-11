// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Represents an asset before being loaded. Used mostly for asset upgrading.
    /// </summary>
    public class PackageLoadingAssetFile
    {
        public readonly UFile FilePath;
        public readonly UDirectory SourceFolder;
        public readonly UFile ProjectFile;

        // If asset has been created or upgraded in place during package upgrade phase, it will be stored here
        public byte[] AssetContent { get; set; }

        public bool Deleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageLoadingAssetFile"/> class.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="sourceFolder">The source folder.</param>
        public PackageLoadingAssetFile(UFile filePath, UDirectory sourceFolder)
        {
            FilePath = filePath;
            SourceFolder = sourceFolder;
            ProjectFile = null;
        }

        public PackageLoadingAssetFile(UFile filePath, UDirectory sourceFolder, UFile projectFile)
        {
            FilePath = filePath;
            SourceFolder = sourceFolder;
            ProjectFile = projectFile;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PackageLoadingAssetFile" /> class.
        /// </summary>
        /// <param name="package">The package this asset will be part of.</param>
        /// <param name="filePath">The relative file path (from default asset folder).</param>
        /// <param name="sourceFolder">The source folder (optional, can be null).</param>
        /// <exception cref="System.ArgumentException">filePath must be relative</exception>
        public PackageLoadingAssetFile(Package package, UFile filePath, UDirectory sourceFolder)
        {
            if (filePath.IsAbsolute)
                throw new ArgumentException("filePath must be relative", filePath);

            SourceFolder = UPath.Combine(package.RootDirectory, sourceFolder ?? package.GetDefaultAssetFolder());
            FilePath = UPath.Combine(SourceFolder, filePath);
            ProjectFile = null;
        }

        internal Stream OpenStream()
        {
            if (Deleted)
                throw new InvalidOperationException();

            if (AssetContent != null)
                return new MemoryStream(AssetContent);

            return new FileStream(FilePath.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = FilePath.MakeRelative(SourceFolder).ToString();
            if (AssetContent != null)
                result += " (Modified)";
            else if (Deleted)
                result += " (Deleted)";

            return result;
        }
    }
}