// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Legacy
{
    /// <summary>
    /// Provides capability to present a <c>Visual</c> element in a <c>HostVisual</c>.
    /// <remarks>The <c>Visual</c> element can be instanced (and thus rendered) in a different <c>Thread</c>.</remarks>
    /// </summary>
    public class VisualTargetPresentationSource : PresentationSource
    {
        private VisualTarget visualTarget;

        /// <summary>
        /// Initializes the <c>PresentationSource</c> to render to the given <c>HostVisual</c>.
        /// <remarks>The <c>HostVisual</c> represents a rendering viewport and must be instanced in the main UI thread.</remarks>
        /// </summary>
        /// <param name="hostVisual">The <c>HostVisual</c> to which the <c>PresentationSource</c> will render.</param>
        public VisualTargetPresentationSource(HostVisual hostVisual)
        {
            visualTarget = new VisualTarget(hostVisual);
        }

        /// <summary>
        /// Gets the <c>CompositionTarget</c> to which WPF will render.
        /// </summary>
        /// <returns>Returns a <c>CompositionTarget</c> use by WPF to render.</returns>
        protected override CompositionTarget GetCompositionTargetCore()
        {
            return visualTarget;
        }

        /// <summary>
        /// Disposal of this <c>PresentationSource is not supported.
        /// It always return false.
        /// </summary>
        public override bool IsDisposed
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// The top-level <c>Visual</c> that will be presented (rendered) by the <c>PresentationSource</c>.
        /// </summary>
        public override Visual RootVisual
        {
            get
            {
                return visualTarget.RootVisual;
            }
            set
            {
                Visual previousRootVisual = visualTarget.RootVisual;
                visualTarget.RootVisual = value;

                // tells the PresentationSource that the top-level Visual has changed
                RootChanged(previousRootVisual, value);

                // need to perform the measurement and arrangement phases manually (if root visual is an UIElement)
                UIElement element = value as UIElement;
                if (element != null)
                {
                    element.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    element.Arrange(new Rect(element.DesiredSize));
                }
            }
        }
    }
}
