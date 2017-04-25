// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using System.ComponentModel;
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

        /// <summary>
        /// The render group for this component.
        /// </summary>
        [DataMember(10)]
        [DefaultValue(RenderGroup.Group0)]
        public RenderGroup RenderGroup { get; set; }

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
