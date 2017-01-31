using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Quantum
{
    public class AssetMemberNodeChangeEventArgs : MemberNodeChangeEventArgs
    {
        public AssetMemberNodeChangeEventArgs([NotNull] MemberNodeChangeEventArgs e, OverrideType previousOverride, OverrideType newOverride, ItemId itemId)
            : base(e.Member, e.Index, e.ChangeType, e.OldValue, e.NewValue)
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
