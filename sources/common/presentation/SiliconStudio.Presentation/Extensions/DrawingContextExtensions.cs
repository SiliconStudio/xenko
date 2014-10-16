// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace SiliconStudio.Presentation.Extensions
{
    public static class DrawingContextExtensions
    {
        public static void DrawVisual(this DrawingContext drawingContext, Visual visual)
        {
            drawingContext.DrawRectangle(new VisualBrush(visual), null, VisualTreeHelper.GetDescendantBounds(visual));
        }
    }
}
