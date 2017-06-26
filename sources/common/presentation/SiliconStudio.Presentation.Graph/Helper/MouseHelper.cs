// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using SiliconStudio.Presentation.Interop;

namespace SiliconStudio.Presentation.Graph.Helper
{
    internal class MouseHelper
    {
        public static Point GetMousePosition(Visual relativeTo)
        {
            NativeHelper.POINT mouse;
            NativeHelper.GetCursorPos(out mouse);

            System.Windows.Interop.HwndSource presentationSource =
                (System.Windows.Interop.HwndSource)PresentationSource.FromVisual(relativeTo);

            NativeHelper.ScreenToClient(presentationSource.Handle, ref mouse);

            GeneralTransform transform = relativeTo.TransformToAncestor(presentationSource.RootVisual);

            Point offset = transform.Transform(new Point(0, 0));

            return new Point(mouse.X - offset.X, mouse.Y - offset.Y);
        }
    }
}
