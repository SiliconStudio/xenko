using System;
using SiliconStudio.Core;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    public abstract class AssetCompositeHierarchyPropertyGraphDefinition<TAssetPartDesign, TAssetPart> : AssetPropertyGraphDefinition
        where TAssetPart : class, IIdentifiable
        where TAssetPartDesign : class, IAssetPartDesign<TAssetPart>
    {
        /// <inheritdoc/>
        public override bool IsObjectReference(IGraphNode targetNode, Index index, object value)
        {
            if (targetNode is IObjectNode && index.IsEmpty)
                return base.IsObjectReference(targetNode, index, value);

            if (value is TAssetPart)
            {
                // Check if we're the part referenced by a part design - other cases are references
                var member = targetNode as IMemberNode;
                return member == null || member.Parent.Type != typeof(TAssetPartDesign);
            }

            return base.IsObjectReference(targetNode, index, value);
        }
    }
}