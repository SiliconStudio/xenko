// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class Int2Editor : VectorEditorBase<Int2>
    {
        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(int), typeof(Int2Editor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(int), typeof(Int2Editor), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnComponentPropertyChanged, CoerceComponentValue));

        /// <summary>
        /// Gets or sets the X component of the <see cref="Int2"/> associated to this control.
        /// </summary>
        public int X { get { return (int)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// Gets or sets the Y component of the <see cref="Int2"/> associated to this control.
        /// </summary>
        public int Y { get { return (int)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        /// <inheritdoc/>
        protected override void UpdateComponentsFromValue(Int2 value)
        {
            SetCurrentValue(XProperty, value.X);
            SetCurrentValue(YProperty, value.Y);
        }

        /// <inheritdoc/>
        protected override Int2 UpdateValueFromComponent(DependencyProperty property)
        {
            if (property == XProperty)
                return new Int2(X, Value.Y);
            if (property == YProperty)
                return new Int2(Value.X, Y);
              
            throw new ArgumentException("Property unsupported by method UpdateValueFromComponent.");
        }

        /// <inheritdoc/>
        protected override Int2 UpateValueFromFloat(float value)
        {
            return new Int2((int)Math.Round(value, MidpointRounding.AwayFromZero));
        }
    }
}