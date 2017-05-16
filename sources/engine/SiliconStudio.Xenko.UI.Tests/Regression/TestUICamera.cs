// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace SiliconStudio.Xenko.UI.Tests.Regression
{
    public class TestUICamera : TestCamera
    {
        public TestUICamera(GraphicsCompositor graphicsCompositor)
            : base(graphicsCompositor)
        {
        }

        protected override void SetCamera()
        {
            base.SetCamera();
            Camera.NearClipPlane = 1f;
            Camera.FarClipPlane = 10000f;
        }
    }
}
