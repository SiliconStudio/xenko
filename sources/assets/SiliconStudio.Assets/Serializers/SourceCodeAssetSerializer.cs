// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SiliconStudio.Core.Diagnostics;

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

        public object Load(Stream stream, string assetFileExtension, ILogger log, out bool aliasOccurred)
        {
            aliasOccurred = false;
            var type = RegisteredExtensions[assetFileExtension];
            var asset = (SourceCodeAsset)Activator.CreateInstance(type);
            return asset;
        }

        public void Save(Stream stream, object asset, ILogger log)
        {
            ((SourceCodeAsset)asset).Save(stream);
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            return RegisteredExtensions.ContainsKey(assetFileExtension) ? this : null;
        }
    }
}