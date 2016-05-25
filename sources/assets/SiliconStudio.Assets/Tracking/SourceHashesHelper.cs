using System;
using System.Collections.Generic;
using System.Reflection;
using SharpYaml.Serialization;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Assets.Tracking
{
    public static class SourceHashesHelper
    {
        public const string MemberName = "~SourceHashes";

        private static readonly Dictionary<Guid, Dictionary<UFile, ObjectId>> AbsoluteSourceHashes = new Dictionary<Guid, Dictionary<UFile, ObjectId>>();
        private static readonly Dictionary<Guid, Dictionary<UFile, ObjectId>> RelativeSourceHashes = new Dictionary<Guid, Dictionary<UFile, ObjectId>>();

        public static bool HasSourceHashes(Guid assetId)
        {
            lock (AbsoluteSourceHashes)
            {
                Dictionary<UFile, ObjectId> hashes;
                return AbsoluteSourceHashes.TryGetValue(assetId, out hashes) && hashes.Count > 0;
            }
        }

        public static ObjectId FindSourceHash(Guid assetId, UFile file)
        {
            lock (AbsoluteSourceHashes)
            {
                Dictionary<UFile, ObjectId> hashes;
                if (!AbsoluteSourceHashes.TryGetValue(assetId, out hashes))
                    return ObjectId.Empty;

                ObjectId hash;
                hashes.TryGetValue(file, out hash);
                return hash;
            }
        }

        public static void UpdateHash(Guid assetId, UFile file, ObjectId hash)
        {
            lock (AbsoluteSourceHashes)
            {
                Dictionary<UFile, ObjectId> hashes;
                if (!AbsoluteSourceHashes.TryGetValue(assetId, out hashes))
                {
                    hashes = new Dictionary<UFile, ObjectId>();
                    AbsoluteSourceHashes.Add(assetId, hashes);
                }

                hashes[file] = hash;
            }
        }

        public static void RemoveHash(Guid assetId, UFile sourceFile)
        {
            lock (AbsoluteSourceHashes)
            {
                Dictionary<UFile, ObjectId> hashes;
                if (AbsoluteSourceHashes.TryGetValue(assetId, out hashes))
                {
                    hashes.Remove(sourceFile);
                }
            }
        }

        public class SourceHashesDynamicMember : DynamicMemberDescriptorBase
        {
            public const int DefaultOrder = int.MaxValue;

            public static readonly SourceHashesDynamicMember Default = new SourceHashesDynamicMember();

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

            public override object Get(object thisObject)
            {
                var asset = (Asset)thisObject;
                // Id can be empty when the asset is contained in a base.
                if (asset.Id == Guid.Empty)
                    return null;

                lock (AbsoluteSourceHashes)
                {
                    Dictionary<UFile, ObjectId> value;
                    RelativeSourceHashes.TryGetValue(asset.Id, out value);
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

                lock (AbsoluteSourceHashes)
                {
                    RelativeSourceHashes[asset.Id] = sourceHashes;
                }
            }

            public override bool HasSet => true;

        }

        public static void AddSourceHashesMember(SharpYaml.Serialization.Descriptors.ObjectDescriptor objectDescriptor, List<SharpYaml.Serialization.IMemberDescriptor> memberDescriptors)
        {
            var type = objectDescriptor.Type;
            if (!typeof(Asset).IsAssignableFrom(type))
                return;

            memberDescriptors.Add(SourceHashesDynamicMember.Default);
        }

        public static void UpdateUPaths(Guid assetId, UDirectory assetFolder, UPathType convertUPathTo)
        {
            switch (convertUPathTo)
            {
                case UPathType.Absolute:
                    ConvertUPaths(assetId, RelativeSourceHashes, AbsoluteSourceHashes, x => UPath.Combine(assetFolder, x));
                    break;
                case UPathType.Relative:
                    ConvertUPaths(assetId, AbsoluteSourceHashes, RelativeSourceHashes, x => x.MakeRelative(assetFolder));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(convertUPathTo), convertUPathTo, null);
            }
        }

        private static void ConvertUPaths(Guid assetId, Dictionary<Guid, Dictionary<UFile, ObjectId>> from, Dictionary<Guid, Dictionary<UFile, ObjectId>> to, Func<UFile, UFile> converter)
        {
            Dictionary<UFile, ObjectId> fromHashes;
            if (from.TryGetValue(assetId, out fromHashes))
            {
                Dictionary<UFile, ObjectId> toHashes;
                if (!to.TryGetValue(assetId, out toHashes))
                {
                    toHashes = new Dictionary<UFile, ObjectId>();
                    to.Add(assetId, toHashes);
                }

                toHashes.Clear();

                foreach (var fromHAsh in fromHashes)
                {
                    var path = converter(fromHAsh.Key);
                    toHashes[path] = fromHAsh.Value;
                }
            }
        }
    }
}
