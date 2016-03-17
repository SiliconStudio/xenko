using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.SpriteStudio.Runtime;
using SiliconStudio.Xenko.Updater;

namespace SiliconStudio.Xenko.Engine
{
    [DataContract("SpriteStudioComponent")]
    [Display("Sprite Studio", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(SpriteStudioProcessor))]
    [DefaultEntityComponentRenderer(typeof(SpriteStudioRendererProcessor))]
    [DataSerializerGlobal(null, typeof(List<SpriteStudioNodeState>))]
    [ComponentOrder(9900)]
    public sealed class SpriteStudioComponent : ActivableEntityComponent
    {
        [DataMember(1)]
        public SpriteStudioSheet Sheet { get; set; }

        [DataMemberIgnore]
        public SpriteStudioNodeState RootNode;

        [DataMemberIgnore]
        public SpriteStudioSheet CurrentSheet;

        [DataMemberIgnore]
        public bool ValidState;

        [DataMemberIgnore, DataMemberUpdatable]
        public List<SpriteStudioNodeState> Nodes { get; } = new List<SpriteStudioNodeState>();

        [DataMemberIgnore]
        internal List<SpriteStudioNodeState> SortedNodes { get; } = new List<SpriteStudioNodeState>();
    }
}