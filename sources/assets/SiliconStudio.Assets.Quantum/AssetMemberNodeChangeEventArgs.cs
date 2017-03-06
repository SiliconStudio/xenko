using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    public interface IAssetNodeChangeEventArgs : INodeChangeEventArgs
    {
        OverrideType PreviousOverride { get; }

        OverrideType NewOverride { get; }

        ItemId ItemId { get; }
    }

    public class AssetMemberNodeChangeEventArgs : MemberNodeChangeEventArgs, IAssetNodeChangeEventArgs
    {
        public AssetMemberNodeChangeEventArgs([NotNull] MemberNodeChangeEventArgs e, OverrideType previousOverride, OverrideType newOverride, ItemId itemId)
            : base(e.Member, e.OldValue, e.NewValue)
        {
            PreviousOverride = previousOverride;
            NewOverride = newOverride;
            ItemId = itemId;
        }

        public OverrideType PreviousOverride { get; }

        public OverrideType NewOverride { get; }

        public ItemId ItemId { get; }
    }

    public class AssetItemNodeChangeEventArgs : ItemChangeEventArgs, IAssetNodeChangeEventArgs
    {
        public AssetItemNodeChangeEventArgs([NotNull] ItemChangeEventArgs e, OverrideType previousOverride, OverrideType newOverride, ItemId itemId)
            : base(e.Node, e.Index, e.ChangeType, e.OldValue, e.NewValue)
        {
            PreviousOverride = previousOverride;
            NewOverride = newOverride;
            ItemId = itemId;
        }

        public OverrideType PreviousOverride { get; }

        public OverrideType NewOverride { get; }

        public ItemId ItemId { get; }
    }
}
