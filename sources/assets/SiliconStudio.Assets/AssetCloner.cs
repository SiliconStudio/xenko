// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.IO;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Allows to clone an asset or values stored in an asset.
    /// </summary>
    public struct AssetCloner
    {
        private readonly bool referencesAsNull;
        private readonly object streamOrValueType;

        private readonly List<object> invariantObjects;
        public static SerializerSelector ClonerSelector { get; internal set; }
        public static PropertyKey<List<object>> InvariantObjectListProperty = new PropertyKey<List<object>>("InvariantObjectList", typeof(AssetCloner));

        static AssetCloner()
        {
            ClonerSelector = new SerializerSelector();
            ClonerSelector.RegisterProfile("Default");
            ClonerSelector.RegisterProfile("Asset");
            ClonerSelector.RegisterProfile("AssetClone");
            ClonerSelector.ReuseReferences = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetCloner" /> struct.
        /// </summary>
        /// <param name="value">The value to clone.</param>
        /// <param name="keepOnlySealedOverride">if set to <c>true</c> to discard override information except sealed.</param>
        /// <param name="referencesAsNull">if set to <c>true</c>, attached references will be cloned as <c>null</c>.</param>
        public AssetCloner(object value, bool keepOnlySealedOverride = false, bool referencesAsNull = false)
        {
            this.referencesAsNull = referencesAsNull;
            invariantObjects = null;

            // Clone only if value is not a value type
            if (value != null && !value.GetType().IsValueType)
            {
                invariantObjects = new List<object>();
                // TODO: keepOnlySealedOverride is currently ignored
                // TODO Clone is not supporting SourceCodeAsset (The SourceCodeAsset.Text won't be cloned)
                var stream = new MemoryStream();
                var writer = new BinarySerializationWriter(stream);
                writer.Context.SerializerSelector = ClonerSelector;
                var refFlag = referencesAsNull ? ContentSerializerContext.AttachedReferenceSerialization.AsNull
                                               : ContentSerializerContext.AttachedReferenceSerialization.AsSerializableVersion;
                writer.Context.Set(InvariantObjectListProperty, invariantObjects);
                writer.Context.Set(ContentSerializerContext.SerializeAttachedReferenceProperty, refFlag);
                writer.SerializeExtended(value, ArchiveMode.Serialize);
                writer.Flush();

                streamOrValueType = stream;
            }
            else
            {
                streamOrValueType = value;
            }
        }

        /// <summary>
        /// Clones the current value of this cloner with the specified new shadow registry (optional)
        /// </summary>
        /// <returns>A clone of the value associated with this cloner.</returns>
        public object Clone()
        {
            var stream = streamOrValueType as Stream;
            if (stream != null)
            {
                stream.Position = 0;
                var reader = new BinarySerializationReader(stream);
                reader.Context.SerializerSelector = ClonerSelector;
                var refFlag = referencesAsNull ? ContentSerializerContext.AttachedReferenceSerialization.AsNull
                                           : ContentSerializerContext.AttachedReferenceSerialization.AsSerializableVersion;
                reader.Context.Set(InvariantObjectListProperty, invariantObjects);
                reader.Context.Set(ContentSerializerContext.SerializeAttachedReferenceProperty, refFlag);
                object newObject = null;
                reader.SerializeExtended(ref newObject, ArchiveMode.Deserialize);
                return newObject;
            }
            // Else this is a value type, so it is cloned automatically
            return streamOrValueType;
        }

        /// <summary>
        /// Clones the specified asset using asset serialization.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="keepOnlySealedOverride">if set to <c>true</c> to discard override information except sealed.</param>
        /// <param name="referencesAsNull">if set to <c>true</c>, attached references will be cloned as <c>null</c>.</param>
        /// <returns>A clone of the asset.</returns>
        public static object Clone(object asset, bool keepOnlySealedOverride = false, bool referencesAsNull = false)
        {
            if (asset == null)
            {
                return null;
            }
            var cloner = new AssetCloner(asset, keepOnlySealedOverride, referencesAsNull);
            return cloner.Clone();
        }
    }
}