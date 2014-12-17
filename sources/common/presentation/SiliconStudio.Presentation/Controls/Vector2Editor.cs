// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class Vector2Editor : Control
    {
        private bool interlock;
        private bool templateApplied;
        private DependencyProperty initializingProperty;

        /// <summary>
        /// Identifies the <see cref="Vector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VectorProperty = DependencyProperty.Register("Vector", typeof(Vector2), typeof(Vector2Editor), new FrameworkPropertyMetadata(default(Vector2), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVectorPropertyChanged, null, false, UpdateSourceTrigger.Explicit));

        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(float), typeof(Vector2Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCartesianPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(float), typeof(Vector2Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCartesianPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(float), typeof(Vector2Editor), new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPolarPropertyChanged, CoerceLengthValue));

        /// <summary>
        /// Identifies the <see cref="Angle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(float), typeof(Vector2Editor), new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPolarPropertyChanged, CoerceAngleValue));

        /// <summary>
        /// The <see cref="Vector2"/> associated to this control.
        /// </summary>
        public Vector2 Vector { get { return (Vector2)GetValue(VectorProperty); } set { SetValue(VectorProperty, value); } }

        /// <summary>
        /// The X component (in Cartesian coordinate system) of the <see cref="Vector2"/> associated to this control.
        /// </summary>
        public float X { get { return (float)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// The Y component (in Cartesian coordinate system) of the <see cref="Vector2"/> associated to this control.
        /// </summary>
        public float Y { get { return (float)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        /// <summary>
        /// The length (in polar coordinate system) of the <see cref="Vector2"/> associated to this control.
        /// </summary>
        public float Length { get { return (float)GetValue(LengthProperty); } set { SetValue(LengthProperty, value); } }

        /// <summary>
        /// The angle (in polar coordinate system) of the <see cref="Vector2"/> associated to this control.
        /// </summary>
        public float Angle { get { return (float)GetValue(AngleProperty); } set { SetValue(AngleProperty, value); } }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            templateApplied = false;
            base.OnApplyTemplate();
            templateApplied = true;
        }

        /// <summary>
        /// Raised when the <see cref="Vector"/> property is modified.
        /// </summary>
        private void OnVectorValueChanged()
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = VectorProperty;
            
            if (!interlock)
            {
                interlock = true;
                SetCurrentValue(XProperty, Vector.X);
                SetCurrentValue(YProperty, Vector.Y);
                SetCurrentValue(LengthProperty, Vector.Length());
                SetCurrentValue(AngleProperty, MathUtil.RadiansToDegrees((float)Math.Atan2(Vector.Y, Vector.X)));
                interlock = false;
            }

            UpdateBinding(VectorProperty);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Raised when the <see cref="X"/> or <see cref="Y"/> properties are modified.
        /// </summary>
        /// <param name="e">The dependency property that has changed.</param>
        private void OnCartesianValueChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = e.Property;
            
            if (!interlock)
            {
                interlock = true;
                if (e.Property == XProperty)
                    Vector = new Vector2((float)e.NewValue, Vector.Y);
                else if (e.Property == YProperty)
                    Vector = new Vector2(Vector.X, (float)e.NewValue);
                else
                    throw new ArgumentException("Property unsupported by method OnCartesianPropertyChanged.");

                SetCurrentValue(LengthProperty, Vector.Length());
                SetCurrentValue(AngleProperty, MathUtil.RadiansToDegrees((float)Math.Atan2(Vector.Y, Vector.X)));
                interlock = false;
            }

            UpdateBinding(e.Property);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Raised when the <see cref="Length"/> or <see cref="Angle"/> properties are modified.
        /// </summary>
        /// <param name="e">The dependency property that has changed.</param>
        private void OnPolarValueChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = e.Property;
            
            if (!interlock)
            {
                interlock = true;
                float length = Length;
                float angle = Angle;
                if (e.Property == LengthProperty)
                {
                    length = (float)e.NewValue;
                }
                else if (e.Property == AngleProperty)
                {
                    angle = (float)e.NewValue;
                }
                else
                    throw new ArgumentException("Property unsupported by method OnCartesianPropertyChanged.");

                angle = MathUtil.DegreesToRadians(angle);
                Vector = new Vector2((float)(length * Math.Cos(angle)), (float)(length * Math.Sin(MathUtil.DegreesToRadians(angle))));
                SetCurrentValue(XProperty, Vector.X);
                SetCurrentValue(YProperty, Vector.Y);
                interlock = false;
            }

            UpdateBinding(e.Property);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Updates the binding of the given dependency property.
        /// </summary>
        /// <param name="dependencyProperty">The dependency property.</param>
        private void UpdateBinding(DependencyProperty dependencyProperty)
        {
            if (dependencyProperty != initializingProperty)
            {
                BindingExpression expression = GetBindingExpression(dependencyProperty);
                if (expression != null)
                    expression.UpdateSource();
            }
        }

        /// <summary>
        /// Raised by <see cref="VectorProperty"/> when the <see cref="Vector"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnVectorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (Vector2Editor)sender;
            editor.OnVectorValueChanged();
        }

        /// <summary>
        /// Raised by <see cref="XProperty"/> or <see cref="YProperty"/> when respectively the
        /// <see cref="X"/> or <see cref="Y"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnCartesianPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (Vector2Editor)sender;
            editor.OnCartesianValueChanged(e);
        }

        /// <summary>
        /// Raised by <see cref="LengthProperty"/> or <see cref="AngleProperty"/> when respectively the
        /// <see cref="Length"/> or <see cref="Angle"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnPolarPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (Vector2Editor)sender;
            editor.OnPolarValueChanged(e);
        }

        /// <summary>
        /// Coerce the value of the Length so it is always positive
        /// </summary>
        private static object CoerceLengthValue(DependencyObject sender, object baseValue)
        {
            return Math.Max(0.0f, (float)baseValue);
        }

        /// <summary>
        /// Coerce the value of the Angle so it is always contained between 0 and 360
        /// </summary>
        private static object CoerceAngleValue(DependencyObject sender, object baseValue)
        {
            return (float)baseValue % 360.0f;
        }
    }
}