// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class Vector2Editor : VectorEditor<Vector2>
    {
        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(float), typeof(Vector2Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(float), typeof(Vector2Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register(nameof(Length), typeof(float), typeof(Vector2Editor), new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceLengthValue));

        static Vector2Editor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Vector2Editor), new FrameworkPropertyMetadata(typeof(Vector2Editor)));
        }

        /// <summary>
        /// Gets or sets the X component (in Cartesian coordinate system) of the <see cref="Vector2"/> associated to this control.
        /// </summary>
        public float X { get { return (float)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// Gets or sets the Y component (in Cartesian coordinate system) of the <see cref="Vector2"/> associated to this control.
        /// </summary>
        public float Y { get { return (float)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        /// <summary>
        /// Gets or sets the length (in polar coordinate system) of the <see cref="Vector2"/> associated to this control.
        /// </summary>
        public float Length { get { return (float)GetValue(LengthProperty); } set { SetValue(LengthProperty, value); } }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Vector2 value)
        {
            SetCurrentValue(XProperty, value.X);
            SetCurrentValue(YProperty, value.Y);
            SetCurrentValue(LengthProperty, value.Length());
        }

        /// <inheritdoc/>
        protected override Vector2 UpdateValueFromComponent(DependencyProperty property)
        {
            switch (EditingMode)
            {
                case VectorEditingMode.Normal:
                    if (property == XProperty)
                        return new Vector2(X, Value.Y);
                    if (property == YProperty)
                        return new Vector2(Value.X, Y);
                    break;

                case VectorEditingMode.AllComponents:
                    if (property == XProperty)
                        return new Vector2(X);
                    if (property == YProperty)
                        return new Vector2(Y);
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
        protected override Vector2 UpateValueFromFloat(float value)
        {
            return new Vector2(value);
        }

        /// <summary>
        /// Coerce the value of the Length so it is always positive
        /// </summary>
        private static object CoerceLengthValue(DependencyObject sender, object baseValue)
        {
            baseValue = CoerceComponentValue(sender, baseValue);
            return Math.Max(0.0f, (float)baseValue);
        }

        private static Vector2 FromLength(Vector2 value, float length)
        {
            var newValue = value;
            newValue.Normalize();
            newValue *= length;
            return newValue;
        }
    }
}