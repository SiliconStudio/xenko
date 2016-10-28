using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum.Contents;

namespace SiliconStudio.Assets.Editor.ViewModel.Quantum
{
    public class AssetContentChangeEventArgs : GraphContentChangeEventArgs
    {
        public AssetContentChangeEventArgs(ContentChangeEventArgs e, OverrideType previousOverride, OverrideType newOverride)
            : base(e.Content, e.Index, e.ChangeType, e.OldValue, e.NewValue)
        {
            PreviousOverride = previousOverride;
            NewOverride = newOverride;
        }

        public OverrideType PreviousOverride { get; }

        public OverrideType NewOverride { get; }
    }
}
