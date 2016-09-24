// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Main entry point for serializing/deserializing <see cref="Asset"/>.
    /// </summary>
    public static class AssetSerializer
    {
        private static readonly List<IAssetSerializerFactory> RegisteredSerializerFactories = new List<IAssetSerializerFactory>();

        /// <summary>
        /// The default serializer.
        /// </summary>
        public static readonly IAssetSerializer Default = new AssetYamlSerializer();

        static AssetSerializer()
        {
            Register((IAssetSerializerFactory)Default);
            Register(SourceCodeAssetSerializer.Default);
        }

        /// <summary>
        /// Registers the specified serializer factory.
        /// </summary>
        /// <param name="serializerFactory">The serializer factory.</param>
        /// <exception cref="ArgumentNullException">serializerFactory</exception>
        public static void Register(IAssetSerializerFactory serializerFactory)
        {
            if (serializerFactory == null) throw new ArgumentNullException(nameof(serializerFactory));
            if (!RegisteredSerializerFactories.Contains(serializerFactory))
                RegisteredSerializerFactories.Add(serializerFactory);
        }

        /// <summary>
        /// Finds a serializer for the specified asset file extension.
        /// </summary>
        /// <param name="assetFileExtension">The asset file extension.</param>
        /// <returns>IAssetSerializerFactory.</returns>
        public static IAssetSerializer FindSerializer(string assetFileExtension)
        {
            if (assetFileExtension == null) throw new ArgumentNullException(nameof(assetFileExtension));
            assetFileExtension = assetFileExtension.ToLowerInvariant();
            for (var i = RegisteredSerializerFactories.Count - 1; i >= 0; i--)
            {
                var assetSerializerFactory = RegisteredSerializerFactories[i];
                var factory = assetSerializerFactory.TryCreate(assetFileExtension);
                if (factory != null)
                {
                    return factory;
                }
            }
            return null;
        }

        /// <summary>
        /// Deserializes an <see cref="Asset" /> from the specified file path.
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="log">The logger.</param>
        /// <returns>An instance of Asset not a valid asset asset object file.</returns>
        public static T Load<T>(string filePath, ILogger log = null)
            where T : Asset
        {
            bool aliasOccurred;
            return (T)Load(filePath, log, out aliasOccurred);
        }

        /// <summary>
        /// Deserializes an <see cref="Asset" /> from the specified file path.
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="filePath">The file path.</param>
        /// <param name="log">The logger.</param>
        /// <param name="aliasOccurred">if set to <c>true</c> an alias on a class/field/property/enum name occurred (rename/remap).</param>
        /// <returns>An instance of Asset not a valid asset asset object file.</returns>
        public static T Load<T>(string filePath, ILogger log, out bool aliasOccurred)
            where T : Asset
        {
            return (T)Load(filePath, log, out aliasOccurred);
        }

        /// <summary>
        /// Deserializes an <see cref="Asset" /> from the specified stream.
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="stream">The stream.</param>
        /// <param name="assetFileExtension">The asset file extension expected when loading the asset (use to find a <see cref="IAssetSerializer" /> with <see cref="IAssetSerializerFactory" />).</param>
        /// <param name="log">The logger.</param>
        /// <returns>An instance of Asset not a valid asset asset object file.</returns>
        /// <exception cref="ArgumentNullException">assetFileExtension</exception>
        /// <exception cref="InvalidOperationException">Unable to find a serializer for [{0}].ToFormat(assetFileExtension)</exception>
        public static T Load<T>(Stream stream, string assetFileExtension, ILogger log = null)
            where T : Asset
        {
            bool aliasOccurred;
            return (T)Load(stream, assetFileExtension, log, out aliasOccurred);
        }

        /// <summary>
        /// Deserializes an <see cref="Asset" /> from the specified stream.
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="stream">The stream.</param>
        /// <param name="assetFileExtension">The asset file extension expected when loading the asset (use to find a <see cref="IAssetSerializer" /> with <see cref="IAssetSerializerFactory" />).</param>
        /// <param name="log">The logger.</param>
        /// <param name="aliasOccurred">if set to <c>true</c> an alias on a class/field/property/enum name occurred (rename/remap).</param>
        /// <returns>An instance of Asset not a valid asset asset object file.</returns>
        /// <exception cref="ArgumentNullException">assetFileExtension</exception>
        /// <exception cref="InvalidOperationException">Unable to find a serializer for [{0}].ToFormat(assetFileExtension)</exception>
        public static T Load<T>(Stream stream, string assetFileExtension, ILogger log, out bool aliasOccurred)
            where T : Asset
        {
            return (T)Load(stream, assetFileExtension, log, out aliasOccurred);
        }

        /// <summary>
        /// Deserializes an <see cref="Asset" /> from the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="log">The logger.</param>
        /// <param name="aliasOccurred">if set to <c>true</c> an alias on a class/field/property/enum name occurred (rename/remap).</param>
        /// <returns>An instance of Asset not a valid asset asset object file.</returns>
        public static Asset Load(string filePath, ILogger log, out bool aliasOccurred)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Load(stream, Path.GetExtension(filePath), log, out aliasOccurred);
            }
        }

        /// <summary>
        /// Deserializes an <see cref="Asset" /> from the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="assetFileExtension">The asset file extension expected when loading the asset (use to find a <see cref="IAssetSerializer" /> with <see cref="IAssetSerializerFactory" />).</param>
        /// <param name="log">The logger.</param>
        /// <param name="aliasOccurred">if set to <c>true</c> an alias on a class/field/property/enum name occurred (rename/remap).</param>
        /// <returns>An instance of Asset not a valid asset asset object file.</returns>
        /// <exception cref="ArgumentNullException">assetFileExtension</exception>
        /// <exception cref="InvalidOperationException">Unable to find a serializer for [{0}].ToFormat(assetFileExtension)</exception>
        public static Asset Load(Stream stream, string assetFileExtension, ILogger log, out bool aliasOccurred)
        {
            if (assetFileExtension == null) throw new ArgumentNullException(nameof(assetFileExtension));
            assetFileExtension = assetFileExtension.ToLowerInvariant();

            var serializer = FindSerializer(assetFileExtension);
            if (serializer == null)
            {
                throw new InvalidOperationException($"Unable to find a serializer for [{assetFileExtension}]");
            }
            return (Asset)serializer.Load(stream, assetFileExtension, log, out aliasOccurred);
        }

        /// <summary>
        /// Serializes an <see cref="Asset" /> to the specified file path.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="asset">The asset object.</param>
        /// <param name="log">The logger.</param>
        /// <exception cref="ArgumentNullException">filePath</exception>
        public static void Save(string filePath, object asset, ILogger log = null)
        {
            if (filePath == null) throw new ArgumentNullException(nameof(filePath));

            // Creates automatically the directory when saving an asset.
            filePath = FileUtility.GetAbsolutePath(filePath);
            var directoryPath = Path.GetDirectoryName(filePath);
            if (directoryPath != null && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var stream = new MemoryStream())
            {
                Save(stream, asset, log);
                File.WriteAllBytes(filePath, stream.ToArray());
            }
        }

        /// <summary>
        /// Serializes an <see cref="Asset" /> to the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="asset">The asset object.</param>
        /// <param name="log">The logger.</param>
        /// <exception cref="ArgumentNullException">
        /// stream
        /// or
        /// assetFileExtension
        /// </exception>
        public static void Save(Stream stream, object asset, ILogger log = null)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (asset == null) return;

            var assetFileExtension = AssetRegistry.GetDefaultExtension(asset.GetType());
            if (assetFileExtension == null)
            {
                throw new ArgumentException("Unable to find a serializer for the specified asset. No asset file extension registered to AssetRegistry");
            }

            var serializer = FindSerializer(assetFileExtension);
            if (serializer == null)
            {
                throw new InvalidOperationException($"Unable to find a serializer for [{assetFileExtension}]");
            }
            serializer.Save(stream, asset, log);
        }
    }
}
