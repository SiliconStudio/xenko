// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SharpYaml.Serialization;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Yaml;
using LogLevel = SharpYaml.Serialization.Logging.LogLevel;

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

        public object Load(Stream stream, string assetFileExtension, ILogger log)
        {
            var type = RegisteredExtensions[assetFileExtension];
            var asset = (SourceCodeAsset)Activator.CreateInstance(type);
            asset.Text = new StreamReader(stream).ReadToEnd();
            return asset;
        }

        public void Save(Stream stream, object asset, ILogger log)
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
        public object Load(Stream stream, string assetFileExtension, ILogger log)
        {
            return YamlSerializer.Deserialize(stream, null, new SerializerContextSettings() { Logger = log != null ? new Logger(log) : null });
        }

        public void Save(Stream stream, object asset, ILogger log)
        {
            YamlSerializer.Serialize(stream, asset, null, new SerializerContextSettings() { Logger = log != null ? new Logger(log) : null });
        }

        public IAssetSerializer TryCreate(string assetFileExtension)
        {
            return this;
        }

        class Logger : SharpYaml.Serialization.Logging.ILogger
        {
            private readonly ILogger logger;

            public Logger(ILogger logger)
            {
                this.logger = logger;
            }

            public void Log(LogLevel level, Exception ex, string message)
            {
                LogMessageType levelConverted;
                switch (level)
                {
                    case LogLevel.Error:
                        levelConverted = LogMessageType.Error;
                        break;
                    case LogLevel.Warning:
                        levelConverted = LogMessageType.Warning;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("level");
                }

                // No need to display message for now, usually ex.Message contains enough information
                logger.Log(new LogMessage("Asset", levelConverted, ex.Message));
            }
        }
    }
}