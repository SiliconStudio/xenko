// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{

    [DataContract("NavigationDebugCameraRenderMode")]
    [NonInstantiable]
    public class NavigationDebugCameraRenderMode : CameraRenderModeBase
    {
        [DataMemberIgnore]
        public RenderStage NavigationDebugRenderStage { get; set; }

        protected override void InitializeCore()
        {
            base.InitializeCore();

            if (NavigationDebugRenderStage == null)
                NavigationDebugRenderStage = RenderSystem.GetOrCreateRenderStage(
                    "NavigationDebugShape", "NavigationDebugShape", 
                    new RenderOutputDescription(PixelFormat.R32G32B32A32_Float, PixelFormat.D24_UNorm_S8_UInt));

            if (NavigationDebugRenderStage != null)
            {
                MainRenderView.RenderStages.Add(NavigationDebugRenderStage);
            }
        }

        protected override void DrawCore(RenderDrawContext context)
        {
            var renderFrame = context.RenderContext.Tags.Get(RenderFrame.Current);
            context.CommandList.Clear(renderFrame.DepthStencil, DepthStencilClearOptions.DepthBuffer);
            RenderSystem.Draw(context, MainRenderView, NavigationDebugRenderStage);
        }
    }
}
