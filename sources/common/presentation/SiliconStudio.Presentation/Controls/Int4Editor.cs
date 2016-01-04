// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class Int4Editor : VectorEditorBase<Int4>
    {
        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(int), typeof(Int4Editor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(int), typeof(Int4Editor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Z"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZProperty = DependencyProperty.Register("Z", typeof(int), typeof(Int4Editor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="W"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WProperty = DependencyProperty.Register("W", typeof(int), typeof(Int4Editor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Gets or sets the X component of the <see cref="Int4"/> associated to this control.
        /// </summary>
        public int X { get { return (int)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// Gets or sets the Y component of the <see cref="Int4"/> associated to this control.
        /// </summary>
        public int Y { get { return (int)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        /// <summary>
        /// Gets or sets the Z component of the <see cref="Int4"/> associated to this control.
        /// </summary>
        public int Z { get { return (int)GetValue(ZProperty); } set { SetValue(ZProperty, value); } }

        /// <summary>
        /// Gets or sets the W component of the <see cref="Int4"/> associated to this control.
        /// </summary>
        public int W { get { return (int)GetValue(WProperty); } set { SetValue(WProperty, value); } }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Int4 value)
        {
            SetCurrentValue(XProperty, value.X);
            SetCurrentValue(YProperty, value.Y);
            SetCurrentValue(ZProperty, value.Z);
            SetCurrentValue(WProperty, value.W);
        }

        /// <inheritdoc/>
        protected override Int4 UpdateValueFromComponent(DependencyProperty property)
        {
            if (property == XProperty)
                return new Int4(X, Value.Y, Value.Z, Value.W);
            if (property == YProperty)
                return new Int4(Value.X, Y, Value.Z, Value.W);
            if (property == ZProperty)
                return new Int4(Value.X, Value.Y, Z, Value.W);
            if (property == WProperty)
                return new Int4(Value.X, Value.Y, Value.Z, W);

            throw new ArgumentException("Property unsupported by method UpdateValueFromComponent.");
        }

        /// <inheritdoc/>
        protected override Int4 UpateValueFromFloat(float value)
        {
            return new Int4((int)Math.Round(value, MidpointRounding.AwayFromZero));
        }
    }
}