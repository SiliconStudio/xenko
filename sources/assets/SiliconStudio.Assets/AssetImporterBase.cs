// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.IO;

namespace SiliconStudio.Assets
{
    public abstract class AssetImporterBase : IAssetImporter
    {
        public abstract Guid Id { get; }

        public virtual string Name
        {
            get
            {
                return GetType().Name;
            }
        }

        public abstract string Description { get; }

        public int Order { get; protected set; }

        public abstract string SupportedFileExtensions { get; }

        public virtual bool IsSupportingFile(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            var file = new UFile(filePath);
            if (file.GetFileExtension() == null) return false;

            return FileUtility.GetFileExtensionsAsSet(SupportedFileExtensions).Contains(file.GetFileExtension());
        }

        public abstract AssetImporterParameters GetDefaultParameters(bool isForReImport);

        public abstract IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters);
    }
}