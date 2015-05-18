using System;
using System.Collections.Generic;
using System.Linq;

using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    public class AssetUpgraderCollection
    {
        private struct VersionRange : IComparable<VersionRange>
        {
            private readonly int minimum;
            private readonly int maximum;
            public readonly int Target;

            public VersionRange(int minimum, int maximum, int target)
            {
                this.minimum = minimum;
                this.maximum = maximum;
                Target = target;
            }

            public bool Contains(int value)
            {
                return minimum <= value && value <= maximum;
            }

            public bool Overlap(VersionRange other)
            {
                return minimum < other.maximum && other.minimum < maximum;
            }

            public int CompareTo(VersionRange other)
            {
                return minimum.CompareTo(other.minimum);
            }
        }

        private readonly SortedList<VersionRange, Type> upgraders = new SortedList<VersionRange, Type>();
        private readonly Dictionary<Type, IAssetUpgrader> instances = new Dictionary<Type, IAssetUpgrader>();
        private readonly int currentVersion;

        public AssetUpgraderCollection(Type assetType, int currentVersion)
        {
            this.currentVersion = currentVersion;
            AssetRegistry.AssertAssetType(assetType);
            AssetType = assetType;
        }

        public Type AssetType { get; private set; }

        internal void RegisterUpgrader(Type upgraderType, int startMinVersion, int startMaxVersion, int targetVersion)
        {
            if (targetVersion > currentVersion)
                throw new ArgumentException("The upgrader has a target version higher that the current version.");

            var range = new VersionRange(startMinVersion, startMaxVersion, targetVersion);

            if (upgraders.Any(x => x.Key.Overlap(range)))
            {
                throw new ArgumentException("The upgrader overlaps with another upgrader.");
            }

            upgraders.Add(new VersionRange(startMinVersion, startMaxVersion, targetVersion), upgraderType);
        }

        internal void Validate(int minVersion)
        {
            int version = minVersion;
            foreach (var upgrader in upgraders)
            {
                if (!upgrader.Key.Contains(version))
                    continue;

                version = upgrader.Key.Target;
                if (version == currentVersion)
                    break;
            }

            if (version != currentVersion)
                throw new InvalidOperationException("No upgrader for asset type [{0}] allow to reach version {1}".ToFormat(AssetType.Name, currentVersion));
        }

        public IAssetUpgrader GetUpgrader(int initialVersion, out int targetVersion)
        {
            var upgrader = upgraders.FirstOrDefault(x => x.Key.Contains(initialVersion));
            if (upgrader.Value == null)
                throw new InvalidOperationException("No upgrader found for version {0} of asset type [{1}]".ToFormat(currentVersion, AssetType.Name));
            targetVersion = upgrader.Key.Target;

            IAssetUpgrader result;
            if (!instances.TryGetValue(upgrader.Value, out result))
            {
                // Cache the upgrader instances
                result = (IAssetUpgrader)Activator.CreateInstance(upgrader.Value);
                instances.Add(upgrader.Value, result);
            }
            return result;
        }
    }
}