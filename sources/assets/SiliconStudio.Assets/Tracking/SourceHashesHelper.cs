using System;
using System.Collections.Generic;
using System.Reflection;
using SharpYaml.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Tracking
{
    public static class SourceHashesHelper
    {
        public const string MemberName = "~SourceHashes";

        private static readonly ShadowObjectPropertyKey AbsoluteSourceHashesKey = new ShadowObjectPropertyKey(new object());
        private static readonly ShadowObjectPropertyKey RelativeSourceHashesKey = new ShadowObjectPropertyKey(new object());
        private static readonly object LockObj = new object();

        public static bool HasSourceHashes(Asset asset)
        {
            lock (LockObj)
            {
                var hashes = TryGet(asset, AbsoluteSourceHashesKey);
                return hashes != null && hashes.Count > 0;
            }
        }

        public static ObjectId FindSourceHash(Asset asset, UFile file)
        {
            lock (LockObj)
            {
                var hashes = TryGet(asset, AbsoluteSourceHashesKey);

                if (hashes == null)
                    return ObjectId.Empty;

                ObjectId hash;
                hashes.TryGetValue(file, out hash);
                return hash;
            }
        }

        public static void UpdateHash(Asset asset, UFile file, ObjectId hash)
        {
            lock (LockObj)
            {
                var hashes = GetOrCreate(asset, AbsoluteSourceHashesKey);
                hashes[file] = hash;
            }
        }

        public static void UpdateHashes(Asset asset, IReadOnlyDictionary<UFile, ObjectId> newHashes)
        {
            lock (LockObj)
            {
                var hashes = GetOrCreate(asset, AbsoluteSourceHashesKey);
                hashes.Clear();
                newHashes.ForEach(x => hashes.Add(x.Key, x.Value));
            }
        }

        public static void RemoveHash(Asset asset, UFile sourceFile)
        {
            lock (LockObj)
            {
                var hashes = TryGet(asset, AbsoluteSourceHashesKey);
                hashes?.Remove(sourceFile);
            }
        }

        public static Dictionary<UFile, ObjectId> GetAllHashes(Asset asset)
        {
            var hashes = TryGet(asset, AbsoluteSourceHashesKey);
            var result = new Dictionary<UFile, ObjectId>();
            hashes?.ForEach(x => result.Add(x.Key, x.Value));
            return result;
        }

        private static Dictionary<UFile, ObjectId> TryGet(Asset asset, ShadowObjectPropertyKey key)
        {
            var shadow = ShadowObject.GetOrCreate(asset);
            object obj;
            if (shadow.TryGetValue(key, out obj))
            {
                return (Dictionary<UFile, ObjectId>)obj;
            }
            return null;
        }

        private static Dictionary<UFile, ObjectId> GetOrCreate(Asset asset, ShadowObjectPropertyKey key)
        {
            var shadow = ShadowObject.GetOrCreate(asset);
            object obj;
            if (shadow.TryGetValue(key, out obj))
            {
                return (Dictionary<UFile, ObjectId>)obj;
            }
            var hashes = new Dictionary<UFile, ObjectId>();
            shadow[key] = hashes;
            return hashes;
        }

        private static void SetDictionary(Asset asset, ShadowObjectPropertyKey key, Dictionary<UFile, ObjectId> dictionary)
        {
            var shadow = ShadowObject.GetOrCreate(asset);
            shadow[key] = dictionary;
        }

        internal static void AddSourceHashesMember(SharpYaml.Serialization.Descriptors.ObjectDescriptor objectDescriptor, List<SharpYaml.Serialization.IMemberDescriptor> memberDescriptors)
        {
            var type = objectDescriptor.Type;
            if (!typeof(Asset).IsAssignableFrom(type))
                return;

            memberDescriptors.Add(SourceHashesDynamicMember.Default);
        }

        internal static void UpdateUPaths(Asset asset, UDirectory assetFolder, UPathType convertUPathTo)
        {
            switch (convertUPathTo)
            {
                case UPathType.Absolute:
                    ConvertUPaths(asset, RelativeSourceHashesKey, AbsoluteSourceHashesKey, x => UPath.Combine(assetFolder, x));
                    break;
                case UPathType.Relative:
                    ConvertUPaths(asset, AbsoluteSourceHashesKey, RelativeSourceHashesKey, x => x.MakeRelative(assetFolder));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(convertUPathTo), convertUPathTo, null);
            }
        }

        private static void ConvertUPaths(Asset asset, ShadowObjectPropertyKey from, ShadowObjectPropertyKey to, Func<UFile, UFile> converter)
        {
            var fromHashes = TryGet(asset, from);
            if (fromHashes != null)
            {
                var toHashes = GetOrCreate(asset, to);
                toHashes.Clear();

                foreach (var fromHAsh in fromHashes)
                {
                    var path = converter(fromHAsh.Key);
                    toHashes[path] = fromHAsh.Value;
                }
            }
        }

        internal class SourceHashesDynamicMember : DynamicMemberDescriptorBase
        {
            public const int DefaultOrder = int.MaxValue;

            public static readonly SourceHashesDynamicMember Default = new SourceHashesDynamicMember { ShouldSerialize = x => { var asset = x as Asset; return asset != null && TryGet(asset, AbsoluteSourceHashesKey)?.Count > 0; } };

            static SourceHashesDynamicMember()
            {
                // Safety check, we need to have the asset id deserialized before the source hashes
                var idOrder = typeof(Asset).GetProperty(nameof(Asset.Id)).GetCustomAttribute<DataMemberAttribute>().Order;
                if (idOrder >= DefaultOrder)
                    throw new InvalidOperationException("The order of the Asset.Id property must be lower than the order of the SourceHashes property.");
            }

            public SourceHashesDynamicMember() : base(MemberName, typeof(Dictionary<UFile, ObjectId>))
            {
                Order = DefaultOrder;
            }

            public override bool HasSet => true;

            public override object Get(object thisObject)
            {
                var asset = (Asset)thisObject;
                // Id can be empty when the asset is contained in a base.
                if (asset.Id == Guid.Empty)
                    return null;

                lock (LockObj)
                {
                    var value = TryGet(asset, RelativeSourceHashesKey);
                    if (value == null || value.Count == 0)
                        return null;

                    return value;
                }
            }

            public override void Set(object thisObject, object value)
            {
                if (value == null)
                    return;

                var sourceHashes = (Dictionary<UFile, ObjectId>)value;
                var asset = (Asset)thisObject;
                // Id can be empty when the asset is contained in a base.
                if (asset.Id == Guid.Empty)
                    return;

                lock (LockObj)
                {
                    SetDictionary(asset, RelativeSourceHashesKey, sourceHashes);
                }
            }
        }
    }
}
