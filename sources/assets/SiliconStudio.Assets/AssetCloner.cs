// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.IO;

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Allows to clone an asset or values stored in an asset.
    /// </summary>
    public struct AssetCloner
    {
        private readonly AssetClonerFlags flags;
        private readonly object streamOrValueType;

        private readonly List<object> invariantObjects;
        private readonly object[] objectReferences;

        public static SerializerSelector ClonerSelector { get; internal set; }
        public static PropertyKey<List<object>> InvariantObjectListProperty = new PropertyKey<List<object>>("InvariantObjectList", typeof(AssetCloner));

        static AssetCloner()
        {
            ClonerSelector = new SerializerSelector(true, "Default", "Asset", "AssetClone");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetCloner" /> struct.
        /// </summary>
        /// <param name="value">The value to clone.</param>
        /// <param name="flags">Cloning flags</param>
        private AssetCloner(object value, AssetClonerFlags flags)
        {
            this.flags = flags;
            invariantObjects = null;
            objectReferences = null;

            // Clone only if value is not a value type
            if (value != null && !value.GetType().IsValueType)
            {
                invariantObjects = new List<object>();
                // TODO: keepOnlySealedOverride is currently ignored
                // TODO Clone is not supporting SourceCodeAsset (The SourceCodeAsset.Text won't be cloned)
                var stream = new MemoryStream();
                var writer = new BinarySerializationWriter(stream);
                writer.Context.SerializerSelector = ClonerSelector;
                var refFlag = (flags & AssetClonerFlags.ReferenceAsNull) != 0
                    ? ContentSerializerContext.AttachedReferenceSerialization.AsNull
                    : ContentSerializerContext.AttachedReferenceSerialization.AsSerializableVersion;
                writer.Context.Set(InvariantObjectListProperty, invariantObjects);
                writer.Context.Set(ContentSerializerContext.SerializeAttachedReferenceProperty, refFlag);
                writer.SerializeExtended(value, ArchiveMode.Serialize);
                writer.Flush();

                // Retrieve back object references
                var objectRefs = writer.Context.Get(MemberSerializer.ObjectSerializeReferences);
                if (objectRefs != null)
                {
                    objectReferences = new object[objectRefs.Count];
                    foreach (var objRef in objectRefs)
                    {
                        objectReferences[objRef.Value] = objRef.Key;
                    }
                }

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
        private object Clone()
        {
            var stream = streamOrValueType as Stream;
            if (stream != null)
            {
                stream.Position = 0;
                var reader = new BinarySerializationReader(stream);
                reader.Context.SerializerSelector = ClonerSelector;
                var refFlag = (flags & AssetClonerFlags.ReferenceAsNull) != 0
                    ? ContentSerializerContext.AttachedReferenceSerialization.AsNull
                    : ContentSerializerContext.AttachedReferenceSerialization.AsSerializableVersion;
                reader.Context.Set(InvariantObjectListProperty, invariantObjects);
                reader.Context.Set(ContentSerializerContext.SerializeAttachedReferenceProperty, refFlag);
                reader.Context.Set(MemberSerializer.ObjectDeserializeCallback, OnObjectDeserialized);
                object newObject = null;
                reader.SerializeExtended(ref newObject, ArchiveMode.Deserialize);
                return newObject;
            }
            // Else this is a value type, so it is cloned automatically
            return streamOrValueType;
        }

        private ObjectId GetHashId()
        {
            // This methods use the stream that is already filled-up by the standard binary serialization of the object
            // Here we add ids and overrides metadata informations to the stream in order to calculate an accurate id
            var stream = streamOrValueType as MemoryStream;
            if (stream != null)
            {
                var writer = new BinarySerializationWriter(stream);
                foreach (var objectRef in objectReferences)
                {
                    if (objectRef != null)
                    {
                        var shadowObject = ShadowObject.GetOrCreate(objectRef);
                        if (shadowObject.IsIdentifiable)
                        {
                            // Get the shadow id (may be a non-shadow, so we may duplicate it (e.g Entity)
                            // but it should not be a big deal
                            var id = shadowObject.GetId(objectRef);
                            writer.Write(id);
                        }

                        // Dump all members with overrides informations
                        foreach (var item in shadowObject)
                        {
                            if (item.Key.Item2 == Override.OverrideKey)
                            {
                                // Use the member name to ensure a stable id
                                var memberName = ((IMemberDescriptor)item.Key.Item1).Name;
                                writer.Write(memberName);
                                writer.Write((int)(OverrideType)item.Value);
                            }
                        }
                    }
                }
                writer.Flush();
                stream.Position = 0;

                return ObjectId.FromBytes(stream.ToArray());
            }

            return ObjectId.Empty;
        }

        private void OnObjectDeserialized(int i, object newObject)
        {
            if (objectReferences != null && newObject != null)
            {
                var previousObject = objectReferences[i];
                ShadowObject.Copy(previousObject, newObject);
                if ((flags & AssetClonerFlags.RemoveOverrides) != 0)
                {
                    Override.RemoveFrom(newObject);
                }
            }
        }

        /// <summary>
        /// Clones the specified asset using asset serialization.
        /// </summary>
        /// <param name="asset">The asset.</param>
        /// <param name="flags">Flags used to control the cloning process</param>
        /// <returns>A clone of the asset.</returns>
        public static object Clone(object asset, AssetClonerFlags flags = AssetClonerFlags.None)
        {
            if (asset == null)
            {
                return null;
            }
            var cloner = new AssetCloner(asset, flags);
            var newObject = cloner.Clone();

            // By default, a clone doesn't copy the base/baseParts for Asset
            if ((flags & AssetClonerFlags.KeepBases) == 0)
            {
                var newAsset = newObject as Asset;
                if (newAsset != null)
                {
                    newAsset.Base = null;
                    newAsset.BaseParts = null;
                }
            }
            return newObject;
        }

        /// <summary>
        /// Generates a runtime hash id from the serialization of this asset.
        /// </summary>
        /// <param name="asset">The asset to get the runtime hash id</param>
        /// <param name="flags">Flags used to control the serialization process</param>
        /// <returns>An object id</returns>
        internal static ObjectId ComputeHash(object asset, AssetClonerFlags flags = AssetClonerFlags.None)
        {
            if (asset == null)
            {
                return ObjectId.Empty;
            }

            var cloner = new AssetCloner(asset, flags);
            var result = cloner.GetHashId();
            return result;
        }
    }
}