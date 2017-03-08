using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Assets.Quantum
{
    public interface IBaseToDerivedRegistry
    {
        void RegisterBaseToDerived([CanBeNull] IAssetNode baseNode, [NotNull] IAssetNode derivedNode);

        [CanBeNull]
        IIdentifiable ResolveFromBase([CanBeNull] object baseObjectReference, [NotNull] IAssetNode derivedReferencerNode);
    }
}
