// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class Int3Editor : VectorEditorBase<Int3>
    {
        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(int), typeof(Int3Editor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(int), typeof(Int3Editor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Z"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZProperty = DependencyProperty.Register("Z", typeof(int), typeof(Int3Editor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Gets or sets the X component of the <see cref="Int3"/> associated to this control.
        /// </summary>
        public int X { get { return (int)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// Gets or sets the Y component of the <see cref="Int3"/> associated to this control.
        /// </summary>
        public int Y { get { return (int)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        /// <summary>
        /// Gets or sets the Z component of the <see cref="Int3"/> associated to this control.
        /// </summary>
        public int Z { get { return (int)GetValue(ZProperty); } set { SetValue(ZProperty, value); } }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Int3 value)
        {
            SetCurrentValue(XProperty, value.X);
            SetCurrentValue(YProperty, value.Y);
            SetCurrentValue(ZProperty, value.Z);
        }

        /// <inheritdoc/>
        protected override Int3 UpdateValueFromComponent(DependencyProperty property)
        {
            if (property == XProperty)
                return new Int3(X, Value.Y, Value.Z);
            if (property == YProperty)
                return new Int3(Value.X, Y, Value.Z);
            if (property == ZProperty)
                return new Int3(Value.X, Value.Y, Z);

            throw new ArgumentException("Property unsupported by method UpdateValueFromComponent.");
        }

        /// <inheritdoc/>
        protected override Int3 UpateValueFromFloat(float value)
        {
            return new Int3((int)Math.Round(value, MidpointRounding.AwayFromZero));
        }
    }
}