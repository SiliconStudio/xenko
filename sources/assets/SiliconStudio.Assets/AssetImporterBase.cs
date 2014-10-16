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

        public abstract string SupportedFileExtensions { get; }

        public virtual int DisplayRank
        {
            get
            {
                return 100;
            }
        }

        public abstract AssetImporterParameters GetDefaultParameters(bool isForReImport);

        public abstract IEnumerable<AssetItem> Import(UFile rawAssetPath, AssetImporterParameters importParameters);
    }
}