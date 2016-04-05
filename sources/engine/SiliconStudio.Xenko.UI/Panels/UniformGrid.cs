// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Diagnostics;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.UI.Panels
{
    /// <summary>
    /// Represents the grid where all the rows and columns have an uniform size.
    /// </summary>
    [DebuggerDisplay("UniformGrid - Name={Name}")]
    public class UniformGrid : GridBase
    {
        /// <summary>
        /// The key to the Columns dependency property.
        /// </summary>
        public readonly static PropertyKey<int> ColumnsPropertyKey = new PropertyKey<int>("ColumnsKey", typeof(UniformGrid), DefaultValueMetadata.Static(1), ValidateValueMetadata.New<int>(GridSizeValidator), ObjectInvalidationMetadata.New<int>(InvalidateGridMeasure));

        /// <summary>
        /// The key to the Rows dependency property.
        /// </summary>
        public readonly static PropertyKey<int> RowsPropertyKey = new PropertyKey<int>("RowsKey", typeof(UniformGrid), DefaultValueMetadata.Static(1), ValidateValueMetadata.New<int>(GridSizeValidator), ObjectInvalidationMetadata.New<int>(InvalidateGridMeasure));

        /// <summary>
        /// The key to the Layers dependency property.
        /// </summary>
        public readonly static PropertyKey<int> LayersPropertyKey = new PropertyKey<int>("LayersKey", typeof(UniformGrid), DefaultValueMetadata.Static(1), ValidateValueMetadata.New<int>(GridSizeValidator), ObjectInvalidationMetadata.New<int>(InvalidateGridMeasure));

        /// <summary>
        /// The final size of one cell
        /// </summary>
        private Vector3 finalForOneCell;

        private static void InvalidateGridMeasure(object propertyOwner, PropertyKey<int> propertyKey, int propertyOldValue)
        {
            var element = (UIElement)propertyOwner;
            element.InvalidateMeasure();
        }

        private static void GridSizeValidator(ref int value)
        {
            if (value < 1)
                throw new ArgumentOutOfRangeException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the number of Columns that the <see cref="UniformGrid"/> has.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value must be strictly positive</exception>
        public int Columns
        {
            get { return DependencyProperties.Get(ColumnsPropertyKey); }
            set { DependencyProperties.Set(ColumnsPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the number of Rows that the <see cref="UniformGrid"/> has.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value must be strictly positive</exception>
        public int Rows
        {
            get { return DependencyProperties.Get(RowsPropertyKey); }
            set { DependencyProperties.Set(RowsPropertyKey, value); }
        }

        /// <summary>
        /// Gets or sets the number of Layers that the <see cref="UniformGrid"/> has.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">The value must be strictly positive</exception>
        public int Layers
        {
            get { return DependencyProperties.Get(LayersPropertyKey); }
            set { DependencyProperties.Set(LayersPropertyKey, value); }
        }

        protected override Vector3 MeasureOverride(Vector3 availableSizeWithoutMargins)
        {
            // compute the size available for one cell
            var gridSize = new Vector3(Columns, Rows, Layers);
            var availableForOneCell = new Vector3(availableSizeWithoutMargins.X / gridSize.X, availableSizeWithoutMargins.Y / gridSize.Y, availableSizeWithoutMargins.Z / gridSize.Z);

            // measure all the children
            var neededForOneCell = Vector3.Zero;
            foreach (var child in VisualChildrenCollection)
            {
                // compute the size available for the child depending on its spans values
                var childSpans = GetElementSpanValuesAsFloat(child);
                var availableForChildWithMargin = Vector3.Modulate(childSpans, availableForOneCell);

                child.Measure(availableForChildWithMargin);

                neededForOneCell = new Vector3(
                    Math.Max(neededForOneCell.X, child.DesiredSizeWithMargins.X / childSpans.X),
                    Math.Max(neededForOneCell.Y, child.DesiredSizeWithMargins.Y / childSpans.Y),
                    Math.Max(neededForOneCell.Z, child.DesiredSizeWithMargins.Z / childSpans.Z));
            }

            return Vector3.Modulate(gridSize, neededForOneCell);
        }

        protected override Vector3 ArrangeOverride(Vector3 finalSizeWithoutMargins)
        {
            // compute the size available for one cell
            var gridSize = new Vector3(Columns, Rows, Layers);
            finalForOneCell = new Vector3(finalSizeWithoutMargins.X / gridSize.X, finalSizeWithoutMargins.Y / gridSize.Y, finalSizeWithoutMargins.Z / gridSize.Z);

            // arrange all the children
            foreach (var child in VisualChildrenCollection)
            {
                // compute the final size of the child depending on its spans values
                var childSpans = GetElementSpanValuesAsFloat(child);
                var finalForChildWithMargin = Vector3.Modulate(childSpans, finalForOneCell);

                // set the arrange matrix of the child
                var childOffsets = GetElementGridPositionsAsFloat(child);
                child.DependencyProperties.Set(PanelArrangeMatrixPropertyKey, Matrix.Translation(Vector3.Modulate(childOffsets, finalForOneCell) - finalSizeWithoutMargins / 2));

                // arrange the child
                child.Arrange(finalForChildWithMargin, IsCollapsed);
            }

            return finalSizeWithoutMargins;
        }
        
        private void CalculateDistanceToSurroundingModulo(float position, float modulo, float elementCount, out Vector2 distances)
        {
            if (modulo <= 0)
            {
                distances = Vector2.Zero;
                return;
            }

            var validPosition = Math.Max(0, Math.Min(position, elementCount * modulo));
            var inferiorQuotient = Math.Min(elementCount - 1, (float)Math.Floor(validPosition / modulo));

            distances.X = (inferiorQuotient+0) * modulo - validPosition;
            distances.Y = (inferiorQuotient+1) * modulo - validPosition;
        }

        public override Vector2 GetSurroudingAnchorDistances(Orientation direction, float position)
        {
            Vector2 distances;
            var gridElements = new Vector3(Columns, Rows, Layers);
            
            CalculateDistanceToSurroundingModulo(position, finalForOneCell[(int)direction], gridElements[(int)direction], out distances);

            return distances;
        }
    }
}
