// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class Vector4Editor : VectorEditor<Vector4>
    {
        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Z"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZProperty = DependencyProperty.Register("Z", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="W"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WProperty = DependencyProperty.Register("W", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceLengthValue));

        static Vector4Editor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Vector4Editor), new FrameworkPropertyMetadata(typeof(Vector4Editor)));
        }

        /// <summary>
        /// Gets or sets the X component (in Cartesian coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float X { get { return (float)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// Gets or sets the Y component (in Cartesian coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float Y { get { return (float)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        /// <summary>
        /// Gets or sets the Z component (in Cartesian coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float Z { get { return (float)GetValue(ZProperty); } set { SetValue(ZProperty, value); } }

        /// <summary>
        /// Gets or sets the W component (in Cartesian coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float W { get { return (float)GetValue(WProperty); } set { SetValue(WProperty, value); } }

        /// <summary>
        /// The length (in polar coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float Length { get { return (float)GetValue(LengthProperty); } set { SetValue(LengthProperty, value); } }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Vector4 value)
        {
            SetCurrentValue(XProperty, value.X);
            SetCurrentValue(YProperty, value.Y);
            SetCurrentValue(ZProperty, value.Z);
            SetCurrentValue(WProperty, value.W);
            SetCurrentValue(LengthProperty, value.Length());
        }

        /// <inheritdoc/>
        protected override Vector4 UpdateValueFromComponent(DependencyProperty property)
        {
            switch (EditingMode)
            {
                case VectorEditingMode.Normal:
                    if (property == XProperty)
                        return new Vector4(X, Value.Y, Value.Z, Value.W);
                    if (property == YProperty)
                        return new Vector4(Value.X, Y, Value.Z, Value.W);
                    if (property == ZProperty)
                        return new Vector4(Value.X, Value.Y, Z, Value.W);
                    if (property == WProperty)
                        return new Vector4(Value.X, Value.Y, Value.Z, W);
                    break;

                case VectorEditingMode.AllComponents:
                    if (property == XProperty)
                        return new Vector4(X);
                    if (property == YProperty)
                        return new Vector4(Y);
                    if (property == ZProperty)
                        return new Vector4(Z);
                    if (property == WProperty)
                        return new Vector4(W);
                    break;

                case VectorEditingMode.Length:
                    if (property == LengthProperty)
                        return FromLength(Value, Length);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(EditingMode));
            }

            throw new ArgumentException($"Property {property} is unsupported by method {nameof(UpdateValueFromComponent)} in {EditingMode} mode.");
        }

        /// <inheritdoc/>
        protected override Vector4 UpateValueFromFloat(float value)
        {
            return new Vector4(value);
        }

        /// <summary>
        /// Coerce the value of the Length so it is always positive
        /// </summary>
        private static object CoerceLengthValue(DependencyObject sender, object baseValue)
        {
            baseValue = CoerceComponentValue(sender, baseValue);
            return Math.Max(0.0f, (float)baseValue);
        }

        private static Vector4 FromLength(Vector4 value, float length)
        {
            var newValue = value;
            newValue.Normalize();
            newValue *= length;
            return newValue;
        }
    }
}