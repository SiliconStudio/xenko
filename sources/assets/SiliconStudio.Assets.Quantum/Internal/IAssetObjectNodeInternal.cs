using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;

namespace SiliconStudio.Assets.Quantum.Internal
{
    /// <summary>
    /// An interface exposing internal methods of <see cref="IAssetObjectNode"/>
    /// </summary>
    internal interface IAssetObjectNodeInternal : IAssetObjectNode, IAssetNodeInternal
    {
        OverrideType GetItemOverride(Index index);

        OverrideType GetKeyOverride(Index index);

        void NotifyOverrideChanging();

        void NotifyOverrideChanged();
    }
}
