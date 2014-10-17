// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets.Serializers
{
    internal class SourceCodeAssetSerializer : IAssetSerializer, IAssetSerializerFactory
    {
        private static readonly Dictionary<string, Type> RegisteredExtensions = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

        public static readonly SourceCodeAssetSerializer Default = new SourceCodeAssetSerializer();

        public static void RegisterExtension(Type assetType, string assetFileExtension)
        {
            if (assetFileExtension == null) throw new ArgumentNullException("assetFileExtension");
            if (!typeof(SourceCodeAsset).IsAssignableFrom(assetType))
                throw new ArgumentException("Asset type must inherit SourceCodeAsset", "assetType");

            RegisteredExtensions.Add(assetFileExtension, assetType);
        }

        public object Load(Stream stream, string assetFileExtension)
        {
            var type = RegisteredExtensions[assetFileExtension];
            var asset = (SourceCodeAsset)Activator.CreateInstance(type);
            asset.Text = new StreamReader(stream).ReadToEnd();
            return asset;
        }

        public void Save(Stream stream, object asset)
        {
            using (var writer = new StreamWriter(stream, Encoding.UTF8, 16384, true))
            {
                writer.Write(((SourceCodeAsset)asset).Text);
            }
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            return RegisteredExtensions.ContainsKey(assetFileExtension) ? this : null;
        }
    }

    internal class AssetYamlSerializer : IAssetSerializer, IAssetSerializerFactory
    {
        public object Load(Stream stream, string assetFileExtension)
        {
            return YamlSerializer.Deserialize(stream);
        }

        public void Save(Stream stream, object asset)
        {
            YamlSerializer.Serialize(stream, asset);
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            return this;
        }
    }
}