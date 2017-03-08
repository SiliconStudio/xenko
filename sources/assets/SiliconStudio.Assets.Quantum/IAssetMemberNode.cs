using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public interface IAssetMemberNode : IAssetNode, IMemberNode
    {
        bool IsNonIdentifiableCollectionContent { get; }

        bool CanOverride { get; }

        [NotNull]
        new IAssetObjectNode Parent { get; }

        new IAssetObjectNode Target { get; }

        void OverrideContent(bool isOverridden);

        OverrideType GetContentOverride();

        bool IsContentOverridden();

        bool IsContentInherited();
    }
}