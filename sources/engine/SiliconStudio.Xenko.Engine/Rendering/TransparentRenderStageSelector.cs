// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.ComponentModel;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class TransparentRenderStageSelector : RenderStageSelector
    {
        [DefaultValue(RenderGroupMask.Group0)]
        public RenderGroupMask RenderGroup { get; set; } = RenderGroupMask.Group0;

        [DefaultValue(null)]
        public RenderStage OpaqueRenderStage { get; set; }
        [DefaultValue(null)]
        public RenderStage TransparentRenderStage { get; set; }

        public string EffectName { get; set; }
    }
}
