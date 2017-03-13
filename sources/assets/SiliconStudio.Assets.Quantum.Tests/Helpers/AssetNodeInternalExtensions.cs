using SiliconStudio.Assets.Quantum.Internal;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Tests.Helpers
{
    public static class AssetNodeInternalExtensions
    {
        public static OverrideType GetItemOverride(this IAssetNode node, Index index)
        {
            return ((IAssetObjectNodeInternal)node).GetItemOverride(index);
        }

        public static OverrideType GetKeyOverride(this IAssetNode node, Index index)
        {
            return ((IAssetObjectNodeInternal)node).GetKeyOverride(index);
        }
    }
}
