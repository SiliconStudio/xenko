// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class RectangleFEditor : VectorEditorBase<RectangleF>
    {
        /// <summary>
        /// Identifies the <see cref="RectX"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RectXProperty = DependencyProperty.Register("RectX", typeof(float), typeof(RectangleFEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="RectY"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RectYProperty = DependencyProperty.Register("RectY", typeof(float), typeof(RectangleFEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="RectWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RectWidthProperty = DependencyProperty.Register("RectWidth", typeof(float), typeof(RectangleFEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="RectHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RectHeightProperty = DependencyProperty.Register("RectHeight", typeof(float), typeof(RectangleFEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Gets or sets the X component of the <see cref="RectangleF"/> associated to this control.
        /// </summary>
        public float RectX { get { return (float)GetValue(RectXProperty); } set { SetValue(RectXProperty, value); } }

        /// <summary>
        /// Gets or sets the Y component of the <see cref="RectangleF"/> associated to this control.
        /// </summary>
        public float RectY { get { return (float)GetValue(RectYProperty); } set { SetValue(RectYProperty, value); } }

        /// <summary>
        /// Gets or sets the width of the <see cref="RectangleF"/> associated to this control.
        /// </summary>
        public float RectWidth { get { return (float)GetValue(RectWidthProperty); } set { SetValue(RectWidthProperty, value); } }

        /// <summary>
        /// Gets or sets the height of the <see cref="RectangleF"/> associated to this control.
        /// </summary>
        public float RectHeight { get { return (float)GetValue(RectHeightProperty); } set { SetValue(RectHeightProperty, value); } }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(RectangleF value)
        {
            SetCurrentValue(RectXProperty, value.X);
            SetCurrentValue(RectYProperty, value.Y);
            SetCurrentValue(RectWidthProperty, value.Width);
            SetCurrentValue(RectHeightProperty, value.Height);
        }

        /// <inheritdoc/>
        protected override RectangleF UpdateValueFromComponent(DependencyProperty property)
        {
            if (property == RectXProperty)
                return new RectangleF(RectX, Value.Y, Value.Width, Value.Height);
            if (property == RectYProperty)
                return new RectangleF(Value.X, RectY, Value.Width, Value.Height);
            if (property == RectWidthProperty)
                return new RectangleF(Value.X, Value.Y, RectWidth, Value.Height);
            if (property == RectHeightProperty)
                return new RectangleF(Value.X, Value.Y, Value.Width, RectHeight);

            throw new ArgumentException("Property unsupported by method UpdateValueFromComponent.");
        }

        /// <inheritdoc/>
        protected override RectangleF UpateValueFromFloat(float value)
        {
            return new RectangleF(0.0f, 0.0f, value, value);
        }
    }
}