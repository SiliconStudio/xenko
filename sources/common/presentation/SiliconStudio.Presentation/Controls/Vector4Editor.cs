// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class Vector4Editor : Control
    {
        private bool interlock;
        private bool templateApplied;
        private DependencyProperty initializingProperty;

        /// <summary>
        /// Identifies the <see cref="Vector"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty VectorProperty = DependencyProperty.Register("Vector", typeof(Vector4), typeof(Vector4Editor), new FrameworkPropertyMetadata(default(Vector4), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVectorPropertyChanged, null, false, UpdateSourceTrigger.Explicit));

        /// <summary>
        /// Identifies the <see cref="X"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty XProperty = DependencyProperty.Register("X", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCartesianPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Y"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty YProperty = DependencyProperty.Register("Y", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCartesianPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Z"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ZProperty = DependencyProperty.Register("Z", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCartesianPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="W"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WProperty = DependencyProperty.Register("W", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCartesianPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Length"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LengthProperty = DependencyProperty.Register("Length", typeof(float), typeof(Vector4Editor), new FrameworkPropertyMetadata(0.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPolarPropertyChanged, CoerceLengthValue));

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            templateApplied = false;
            base.OnApplyTemplate();
            templateApplied = true;
        }

        /// <summary>
        /// The <see cref="Vector4"/> associated to this control.
        /// </summary>
        public Vector4 Vector { get { return (Vector4)GetValue(VectorProperty); } set { SetValue(VectorProperty, value); } }

        /// <summary>
        /// The X component (in Cartesian coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float X { get { return (float)GetValue(XProperty); } set { SetValue(XProperty, value); } }

        /// <summary>
        /// The Y component (in Cartesian coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float Y { get { return (float)GetValue(YProperty); } set { SetValue(YProperty, value); } }

        /// <summary>
        /// The Y component (in Cartesian coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float Z { get { return (float)GetValue(ZProperty); } set { SetValue(ZProperty, value); } }

        /// <summary>
        /// The Y component (in Cartesian coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float W { get { return (float)GetValue(WProperty); } set { SetValue(WProperty, value); } }

        /// <summary>
        /// The length (in polar coordinate system) of the <see cref="Vector4"/> associated to this control.
        /// </summary>
        public float Length { get { return (float)GetValue(LengthProperty); } set { SetValue(LengthProperty, value); } }

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
                SetCurrentValue(ZProperty, Vector.Z);
                SetCurrentValue(WProperty, Vector.W);
                SetCurrentValue(LengthProperty, Vector.Length());
                interlock = false;
            }

            UpdateBinding(VectorProperty);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Raised when the <see cref="X"/>, <see cref="Y"/>, <see cref="Z"/> or <see cref="W"/> properties are modified.
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
                    Vector = new Vector4((float)e.NewValue, Vector.Y, Vector.Z, Vector.W);
                else if (e.Property == YProperty)
                    Vector = new Vector4(Vector.X, (float)e.NewValue, Vector.Z, Vector.W);
                else if (e.Property == ZProperty)
                    Vector = new Vector4(Vector.X, Vector.Y, (float)e.NewValue, Vector.W);
                else if (e.Property == WProperty)
                    Vector = new Vector4(Vector.X, Vector.Y, Vector.Z, (float)e.NewValue);
                else
                    throw new ArgumentException("Property unsupported by method OnCartesianPropertyChanged.");

                SetCurrentValue(LengthProperty, Vector.Length());
                interlock = false;
            }

            UpdateBinding(e.Property);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Raised when the <see cref="Length"/> property is modified.
        /// </summary>
        /// <param name="e">The dependency property that has changed.</param>
        private void OnLengthValueChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = LengthProperty;
            
            if (!interlock)
            {
                interlock = true;
                var vector = Vector;
                vector.Normalize();
                vector *= Length;
                Vector = vector;
                SetCurrentValue(XProperty, Vector.X);
                SetCurrentValue(YProperty, Vector.Y);
                SetCurrentValue(ZProperty, Vector.Z);
                SetCurrentValue(WProperty, Vector.W);
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
            var editor = (Vector4Editor)sender;
            editor.OnVectorValueChanged();
        }

        /// <summary>
        /// Raised by <see cref="XProperty"/>, <see cref="YProperty"/>, <see cref="ZProperty"/> or <see cref="WProperty"/>
        /// when respectively the <see cref="X"/>, <see cref="Y"/>, <see cref="Z"/> or <see cref="W"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnCartesianPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (Vector4Editor)sender;
            editor.OnCartesianValueChanged(e);
        }

        /// <summary>
        /// Raised by <see cref="LengthProperty"/> when the <see cref="Length"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnPolarPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (Vector4Editor)sender;
            editor.OnLengthValueChanged(e);
        }

        /// <summary>
        /// Coerce the value of the Length so it is always positive
        /// </summary>
        private static object CoerceLengthValue(DependencyObject sender, object baseValue)
        {
            return Math.Max(0.0f, (float)baseValue);
        }
    }
}