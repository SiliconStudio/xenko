// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Legacy
{
    /// <summary>
    /// Wraps a <c>Visual</c> into an <c>UIElement</c>
    /// </summary>
    [ContentProperty("Child")]
    public class VisualElement : UIElement
    {
        private Visual visual;

        public VisualElement()
        {
        }

        public VisualElement(Visual visual)
        {
            if (visual == null)
                throw new ArgumentNullException("visual");

            Child = visual;
        }

        public Visual Child
        {
            get { return visual; }
            set
            {
                if (visual != null)
                    RemoveVisualChild(visual);

                visual = value;

                if (visual != null)
                    AddVisualChild(visual);
            }
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return visual;
        }
    }
}
