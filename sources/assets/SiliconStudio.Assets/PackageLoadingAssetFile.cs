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
        public UFile FilePath;
        public UDirectory SourceFolder;

        // If asset has been created or upgraded in place during package upgrade phase, it will be stored here
        public byte[] AssetContent;

        public bool Deleted;

        public PackageLoadingAssetFile(UFile filePath, UDirectory sourceFolder)
        {
            FilePath = filePath;
            SourceFolder = sourceFolder;
        }

        internal Stream OpenStream()
        {
            if (Deleted)
                throw new InvalidOperationException();

            if (AssetContent != null)
                return new MemoryStream(AssetContent);

            return new FileStream(FilePath.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}