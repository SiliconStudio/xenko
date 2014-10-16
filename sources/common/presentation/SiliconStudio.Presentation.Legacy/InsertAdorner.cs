// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Legacy
{
    public class InsertAdorner : Adorner
    {
        private AdornerLayer adornerLayer;

        public InsertAdorner(UIElement adornedElement, AdornerLayer adornerLayer)
            : base(adornedElement)
        {
            this.adornerLayer = adornerLayer;

            FocusedElement = adornedElement;
            FocusedElementHeader = adornedElement;

            adornerLayer.Add(this);
        }

        private bool isDirty = true;

        private UIElement focusedElement;
        public UIElement FocusedElement
        {
            get { return focusedElement; }
            set
            {
                if (focusedElement != value)
                {
                    focusedElement = value;
                    isDirty = true;
                }
            }
        }

        public UIElement focusedElementHeader;
        public UIElement FocusedElementHeader
        {
            get { return focusedElementHeader; }
            set
            {
                if (focusedElementHeader != value)
                {
                    focusedElementHeader = value;
                    isDirty = true;
                }
            }
        }

        public bool isTopHalf;
        public bool IsTopHalf
        {
            get { return isTopHalf; }
            set
            {
                if (isTopHalf != value)
                {
                    isTopHalf = value;
                    isDirty = true;
                }
            }
        }

        public void Destroy()
        {
            adornerLayer.Remove(this);
        }

        public void Reevaluate()
        {
            if (isDirty == false)
                return;

            isDirty = false;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double positionY = IsTopHalf ? 0.0f : FocusedElement.RenderSize.Height;
            var position1 = FocusedElementHeader.TranslatePoint(new Point(0.0f, positionY), AdornedElement);
            var position2 = FocusedElementHeader.TranslatePoint(new Point(FocusedElementHeader.RenderSize.Width, positionY), AdornedElement);
            drawingContext.DrawLine(new Pen(Brushes.Black, 2.0f), position1, position2);
        }
    }
}