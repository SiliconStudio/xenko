// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Legacy
{
    /// <summary>
    /// Wraps a several <c>Visual</c>s into a <c>UIElement</c>
    /// </summary>
    public class VisualContainerElement : UIElement
    {
        private readonly List<Visual> visuals = new List<Visual>();

        public void AddVisual(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");

            AddVisualChild(visual);
            //AddLogicalChild(visual);
            visuals.Add(visual);
        }

        public void Clear()
        {
            visuals.ForEach(RemoveVisualChild);
            /*
            visuals.ForEach(visual =>
            {
                // RemoveLogicalChild(visual);
                RemoveVisualChild(visual);
            });
            */
            visuals.Clear();
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return visuals.Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visuals[index];
        }
    }
}
