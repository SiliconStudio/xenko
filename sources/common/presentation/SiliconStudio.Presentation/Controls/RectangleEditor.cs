// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class RectangleEditor : VectorEditorBase<Rectangle>
    {
        /// <summary>
        /// Identifies the <see cref="RectX"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RectXProperty = DependencyProperty.Register("RectX", typeof(int), typeof(RectangleEditor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="RectY"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RectYProperty = DependencyProperty.Register("RectY", typeof(int), typeof(RectangleEditor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="RectWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RectWidthProperty = DependencyProperty.Register("RectWidth", typeof(int), typeof(RectangleEditor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="RectHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RectHeightProperty = DependencyProperty.Register("RectHeight", typeof(int), typeof(RectangleEditor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Gets or sets the X component of the <see cref="Rectangle"/> associated to this control.
        /// </summary>
        public int RectX { get { return (int)GetValue(RectXProperty); } set { SetValue(RectXProperty, value); } }

        /// <summary>
        /// Gets or sets the Y component of the <see cref="Rectangle"/> associated to this control.
        /// </summary>
        public int RectY { get { return (int)GetValue(RectYProperty); } set { SetValue(RectYProperty, value); } }

        /// <summary>
        /// Gets or sets the Width component of the <see cref="Rectangle"/> associated to this control.
        /// </summary>
        public int RectWidth { get { return (int)GetValue(RectWidthProperty); } set { SetValue(RectWidthProperty, value); } }

        /// <summary>
        /// Gets or sets the Height component of the <see cref="Rectangle"/> associated to this control.
        /// </summary>
        public int RectHeight { get { return (int)GetValue(RectHeightProperty); } set { SetValue(RectHeightProperty, value); } }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Rectangle value)
        {
            SetCurrentValue(RectXProperty, value.X);
            SetCurrentValue(RectYProperty, value.Y);
            SetCurrentValue(RectWidthProperty, value.Width);
            SetCurrentValue(RectHeightProperty, value.Height);
        }

        /// <inheritdoc/>
        protected override Rectangle UpdateValueFromComponent(DependencyProperty property)
        {
            if (property == RectXProperty)
                return new Rectangle(RectX, Value.Y, Value.Width, Value.Height);
            if (property == RectYProperty)
                return new Rectangle(Value.X, RectY, Value.Width, Value.Height);
            if (property == RectWidthProperty)
                return new Rectangle(Value.X, Value.Y, RectWidth, Value.Height);
            if (property == RectHeightProperty)
                return new Rectangle(Value.X, Value.Y, Value.Width, RectHeight);

            throw new ArgumentException("Property unsupported by method UpdateValueFromComponent.");
        }

        /// <inheritdoc/>
        protected override Rectangle UpateValueFromFloat(float value)
        {
            var intValue = (int)Math.Round(value, MidpointRounding.AwayFromZero);
            return new Rectangle(0, 0, intValue, intValue);
        }
    }
}