// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
