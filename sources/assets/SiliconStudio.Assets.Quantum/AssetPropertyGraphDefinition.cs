using System;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum
{
    [AssetPropertyGraphDefinition(typeof(Asset))]
    // ReSharper disable once RequiredBaseTypesIsNotInherited - due to a limitation on how ReSharper checks this requirement (see https://youtrack.jetbrains.com/issue/RSRP-462598)
    public class AssetPropertyGraphDefinition
    {
        public virtual bool IsObjectReference(IGraphNode targetNode, Index index, object value)
        {
            return false;
        }
    }
}