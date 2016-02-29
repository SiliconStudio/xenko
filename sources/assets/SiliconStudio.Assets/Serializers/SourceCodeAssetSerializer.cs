// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.IO;
using System.Text;

using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets.Serializers
{
    internal class SourceCodeAssetSerializer : IAssetSerializer, IAssetSerializerFactory
    {
        public static readonly SourceCodeAssetSerializer Default = new SourceCodeAssetSerializer();

        public object Load(Stream stream, string assetFileExtension, ILogger log, out bool aliasOccurred)
        {
            aliasOccurred = false;

            var type = AssetRegistry.GetAssetTypeFromFileExtension(assetFileExtension);
            var asset = (SourceCodeAsset)Activator.CreateInstance(type);

            var reader = new StreamReader(stream, Encoding.UTF8);
            asset.Text = reader.ReadToEnd();

            return asset;
        }

        public void Save(Stream stream, object asset, ILogger log)
        {
            ((SourceCodeAsset)asset).Save(stream);
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            var assetType = AssetRegistry.GetAssetTypeFromFileExtension(assetFileExtension);
            if (assetType != null && typeof(SourceCodeAsset).IsAssignableFrom(assetType))
            {
                return this;
            }
            return null;
        }
    }
}