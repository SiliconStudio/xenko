using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Editor.ViewModel.Quantum
{
    public class AssetContentChangeEventArgs : GraphContentChangeEventArgs
    {
        public AssetContentChangeEventArgs(ContentChangeEventArgs e, MemberFlags previousMemberFlags, MemberFlags newMemberFlags)
            : base(e.Content, e.Index, e.ChangeType, e.OldValue, e.NewValue)
        {
            PreviousMemberFlags = previousMemberFlags;
            NewMemberFlags = newMemberFlags;
        }

        public MemberFlags PreviousMemberFlags { get; }

        public MemberFlags NewMemberFlags { get; }
    }
}
