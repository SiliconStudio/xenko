// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Panels
{
    /// <summary> 
    /// Defines an area within which you can position and size child elements with respect to in the Canvas area size.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("Canvas - Name={Name}")]
    public class Canvas : Panel
    {
        /// <summary>
        /// The key to the RelativeSize dependency property. RelativeSize indicate the ratio of the size of the <see cref="UIElement"/> with respect to the parent size.
        /// </summary>
        /// <remarks>Relative size must be strictly positive</remarks>
        public readonly static PropertyKey<Vector3> RelativeSizePropertyKey = new PropertyKey<Vector3>("RelativeSizeKey", typeof(Canvas), DefaultValueMetadata.Static(new Vector3(float.NaN)), ValidateValueMetadata.New<Vector3>(ValidateRelativeSize), ObjectInvalidationMetadata.New<Vector3>(InvalidateCanvasMeasure));

        /// <summary>
        /// The key to the RelativePosition dependency property. RelativePosition indicate where the <see cref="UIElement"/> is pinned in the canvas.
        /// </summary>
        public readonly static PropertyKey<Vector3> RelativePositionPropertyKey = new PropertyKey<Vector3>("RelativePositionKey", typeof(Canvas), DefaultValueMetadata.Static(new Vector3(0)), ObjectInvalidationMetadata.New<Vector3>(OnRelativePositionChanged));

        /// <summary>
        /// The key to the AbsolutePosition dependency property. AbsolutePosition indicate where the <see cref="UIElement"/> is pinned in the canvas.
        /// </summary>
        public readonly static PropertyKey<Vector3> AbsolutePositionPropertyKey = new PropertyKey<Vector3>("AbsolutePositionKey", typeof(Canvas), DefaultValueMetadata.Static(new Vector3(0)), ObjectInvalidationMetadata.New<Vector3>(OnAbsolutePositionChanged));

        /// <summary>
        /// The key to the useAbsolutionPosition dependency property. This indicates whether to use the AbsolutePosition or the RelativePosition to place to element.
        /// </summary>
        private readonly static PropertyKey<bool> useAbsolutionPositionPropertyKey = new PropertyKey<bool>("useAbsolutionPositionKey", typeof(Canvas), DefaultValueMetadata.Static(false));

        /// <summary>
        /// The key to the PinOrigin dependency property. The PinOrigin indicate which point of the <see cref="UIElement"/> should be pinned to the canvas. 
        /// </summary>
        /// <remarks>
        /// Those values are normalized between 0 and 1. (0,0,0) represent the Left/Top/Back corner and (1,1,1) represent the Right/Bottom/Front corner. 
        /// <see cref="UIElement"/>'s margins are included in the normalization. 
        /// Values beyond [0,1] are clamped.</remarks>
        public readonly static PropertyKey<Vector3> PinOriginPropertyKey = new PropertyKey<Vector3>("PinOriginKey", typeof(Canvas), DefaultValueMetadata.Static(Vector3.Zero), ValidateValueMetadata.New<Vector3>(PinOriginValueValidator), ObjectInvalidationMetadata.New<Vector3>(InvalidateCanvasMeasure));
        
        private static void OnRelativePositionChanged(object propertyOwner, PropertyKey<Vector3> propertyKey, Vector3 propertyOldValue)
        {
            var element = (UIElement)propertyOwner;
            element.DependencyProperties.Set(useAbsolutionPositionPropertyKey, false);

            InvalidateCanvasMeasure(propertyOwner, propertyKey, propertyOldValue);
        }

        private static void OnAbsolutePositionChanged(object propertyOwner, PropertyKey<Vector3> propertyKey, Vector3 propertyOldValue)
        {
            var element = (UIElement)propertyOwner;
            element.DependencyProperties.Set(useAbsolutionPositionPropertyKey, true);

            InvalidateCanvasMeasure(propertyOwner, propertyKey, propertyOldValue);
        }

        protected static void InvalidateCanvasMeasure(object propertyOwner, PropertyKey<Vector3> propertyKey, Vector3 propertyOldValue)
        {
            var element = (UIElement)propertyOwner;
            var parentCanvas = element.Parent as Canvas;

            if (parentCanvas != null)
                parentCanvas.InvalidateMeasure();
        }

        private static void PinOriginValueValidator(ref Vector3 value)
        {
            value.X = Math.Min(1, Math.Max(0, value.X));
            value.Y = Math.Min(1, Math.Max(0, value.Y));
            value.Z = Math.Min(1, Math.Max(0, value.Z));
        }

        private static void ValidateRelativeSize(ref Vector3 value)
        {
            if (value.X < 0 || value.Y < 0 || value.Z < 0)
                throw new InvalidOperationException("All the components of the a relative size must be positive");
        }

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            // Measure all the children
            // Canvas does not take into account possible collisions between children
            // The available size for a child is thus the size of the canvas available after or before the PinPosition depending on the element PinOrigin. 
            foreach (var child in VisualChildrenCollection)
            {
                // calculate the available space for the child
                var childAvailableSizeWithMargin = ComputeAvailableSize(child, availableSizeWithoutMargins, false);

                // override the available space if the child size is relative to its parent's.
                var childRelativeSize = child.DependencyProperties.Get(RelativeSizePropertyKey);
                for (var i = 0; i < 3; i++)
                {
                    if (float.IsNaN(childRelativeSize[i])) // relative size is not set
                        continue;

                    childAvailableSizeWithMargin[i] = childRelativeSize[i] > 0? childRelativeSize[i] * availableSizeWithoutMargins[i]: 0f; // avoid NaN due to 0 x Infinity
                }

                child.Measure(childAvailableSizeWithMargin);
            }

            // Estimate the size needed so that the biggest child fits
            var desiredSizeWithoutMargin = Vector3.Zero;
            foreach (var child in VisualChildrenCollection)
            {                
                if(child.IsCollapsed)
                    continue;

                // determine the position of the right/top/front corner of the child
                var childExtremityCorner = Vector3.Zero;
                var pinOrigin = child.DependencyProperties.Get(PinOriginPropertyKey);
                var childRelativeSize = child.DependencyProperties.Get(RelativeSizePropertyKey);
                var childUseAbsolutionPosition = child.DependencyProperties.Get(useAbsolutionPositionPropertyKey);
                var childAbsolutePosition = child.DependencyProperties.Get(AbsolutePositionPropertyKey);
                var childRelativePosition = child.DependencyProperties.Get(RelativePositionPropertyKey);
                for (var i = 0; i < 3; i++)
                {
                    if (!float.IsNaN(childRelativeSize[i])) // relative size is set
                    {
                        childExtremityCorner[i] = childRelativeSize[i] > 0? child.DesiredSizeWithMargins[i] / childRelativeSize[i]: 0f;
                    }
                    else if (childUseAbsolutionPosition && !float.IsNaN(childAbsolutePosition[i])) // prioritize absolute position and absolute position is set.
                    {
                        childExtremityCorner[i] = childAbsolutePosition[i] + (1f - pinOrigin[i]) * child.DesiredSizeWithMargins[i];
                    }
                    else if (!float.IsNaN(childRelativePosition[i]))
                    {
                        if (pinOrigin[i] > 0 && childRelativePosition[i] > 0)
                            childExtremityCorner[i] = pinOrigin[i] * child.DesiredSizeWithMargins[i] / childRelativePosition[i];
                        if (pinOrigin[i] < 1 && childRelativePosition[i] < 1)
                            childExtremityCorner[i] = Math.Max(childExtremityCorner[i], (1 - pinOrigin[i]) * child.DesiredSizeWithMargins[i] / (1 - childRelativePosition[i]));
                    }
                    else
                    {
                        childExtremityCorner[i] = 0;
                    }
                }

                // increase the parent desired size if one of its children get out of it.
                desiredSizeWithoutMargin = new Vector3(
                    Math.Max(desiredSizeWithoutMargin.X, childExtremityCorner.X),
                    Math.Max(desiredSizeWithoutMargin.Y, childExtremityCorner.Y),
                    Math.Max(desiredSizeWithoutMargin.Z, childExtremityCorner.Z));
            }

            return desiredSizeWithoutMargin;
        }

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            // Arrange all the children
            foreach (var child in VisualChildrenCollection)
            {
                // calculate the size provided to the child
                var availableSize = ComputeAvailableSize(child, finalSizeWithoutMargins, true); //should we force the element size when element relative size is set ???
                var childProvidedSize = new Vector3(
                    Math.Min(availableSize.X, child.DesiredSizeWithMargins.X),
                    Math.Min(availableSize.Y, child.DesiredSizeWithMargins.Y),
                    Math.Min(availableSize.Z, child.DesiredSizeWithMargins.Z));
                
                // arrange the child
                child.Arrange(childProvidedSize, IsCollapsed);

                // compute the child offsets wrt to parent (left,top,front) corner
                var pinOrigin = child.DependencyProperties.Get(PinOriginPropertyKey);
                var childOrigin = ComputeAbsolutePinPosition(child, ref finalSizeWithoutMargins) - Vector3.Modulate(pinOrigin, child.RenderSize);

                // compute the child offsets wrt to parent origin (0,0,0). 
                var childOriginParentCenter = childOrigin - finalSizeWithoutMargins / 2;

                // set the panel arrange matrix for the child
                child.DependencyProperties.Set(PanelArrangeMatrixPropertyKey, Matrix.Translation(childOriginParentCenter));

            }

            return base.ArrangeOverride(finalSizeWithoutMargins);
        }

        /// <summary>
        /// Compute the child absolute position in the canvas according to parent size and the child layout properties.
        /// </summary>
        /// <param name="child">The child to place</param>
        /// <param name="parentSize">The parent size</param>
        /// <returns>The child absolute position offset</returns>
        protected Vector3 ComputeAbsolutePinPosition(UIElement child, ref Vector3 parentSize)
        {
            var relativePosition = child.DependencyProperties.Get(RelativePositionPropertyKey);
            var absolutePosition = child.DependencyProperties.Get(AbsolutePositionPropertyKey);
            var useAbsolutionPosition = child.DependencyProperties.Get(useAbsolutionPositionPropertyKey);

            for (var dim = 0; dim < 3; ++dim)
            {
                if (float.IsNaN(absolutePosition[dim]) || !useAbsolutionPosition && !float.IsNaN(relativePosition[dim]))
                    absolutePosition[dim] = relativePosition[dim] == 0f ? 0f : relativePosition[dim] * parentSize[dim];
            }

            return absolutePosition;
        }

        /// <summary>
        /// Compute the space available to the provided child based on size available to the canvas and the child layout properties.
        /// </summary>
        /// <param name="child">The child of the canvas to measure/arrange</param>
        /// <param name="availableSize">The space available to the canvas</param>
        /// <param name="ignoreRelativeSize">Indicate if the child RelativeSize property should be taken in account or nor</param>
        /// <returns></returns>
        protected Vector3 ComputeAvailableSize(UIElement child, Vector3 availableSize, bool ignoreRelativeSize)
        {
            // calculate the absolute position of the child
            var pinPosition = ComputeAbsolutePinPosition(child, ref availableSize);
            var pinOrigin = child.DependencyProperties.Get(PinOriginPropertyKey);
            var relativeSize = child.DependencyProperties.Get(RelativeSizePropertyKey);
            var childAvailableSize = Vector3.Zero;

            for (var dim = 0; dim < 3; dim++)
            {
                if (!ignoreRelativeSize && !float.IsNaN(relativeSize[dim]))
                {
                    childAvailableSize[dim] = relativeSize[dim] > 0? relativeSize[dim] * availableSize[dim]: 0f;
                }
                else if (pinPosition[dim] < 0 || pinPosition[dim] > availableSize[dim])
                {
                    childAvailableSize[dim] = 0;
                }
                else
                {
                    var availableBeforeElement = float.PositiveInfinity;
                    if (pinPosition[dim] >= 0 && pinOrigin[dim] > 0)
                        availableBeforeElement = pinPosition[dim] / pinOrigin[dim];

                    var availableAfterElement = float.PositiveInfinity;
                    if (pinPosition[dim] <= availableSize[dim] && !float.IsPositiveInfinity(pinPosition[dim]) && pinOrigin[dim] < 1)
                        availableAfterElement = (availableSize[dim] - pinPosition[dim]) / (1f - pinOrigin[dim]);

                    childAvailableSize[dim] = Math.Min(availableBeforeElement, availableAfterElement);
                }
            }

            return childAvailableSize;
        }
    }
}