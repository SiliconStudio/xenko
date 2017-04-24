// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interactivity;
using SiliconStudio.Presentation.Controls;

namespace SiliconStudio.Xenko.DebugTools.Behaviors
{
    public class TimeBarCurrentTimeSetterBehavior : Behavior<ScaleBar>
    {
        public ProcessInfoRenderer Renderer { get; set; }

        protected override void OnAttached()
        {
            if (Renderer == null)
                // throw new InvalidOperationException("The Renderer property must be set a valid value.");
                return; // can be null at design time

            Renderer.LastFrameRender += OnRendererLastFrameRender;
        }

        protected override void OnDetaching()
        {
            Renderer.LastFrameRender -= OnRendererLastFrameRender;
        }

        private void OnRendererLastFrameRender(object sender, FrameRenderRoutedEventArgs e)
        {
            AssociatedObject.SetUnitAt(e.FrameData.EndTime, Renderer.ActualWidth);
        }
    }
}
