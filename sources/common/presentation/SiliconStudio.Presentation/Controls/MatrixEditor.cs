// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Presentation.Controls
{
    public class MatrixEditor : Control
    {
        private static readonly Dictionary<DependencyProperty, int> PropertyToIndex;
        private bool interlock;
        private bool templateApplied;
        private DependencyProperty initializingProperty;

        /// <summary>
        /// Identifies the <see cref="Matrix"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MatrixProperty = DependencyProperty.Register("Matrix", typeof(Matrix), typeof(MatrixEditor), new FrameworkPropertyMetadata(default(Matrix), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnMatrixPropertyChanged, null, false, UpdateSourceTrigger.Explicit));

        /// <summary>
        /// Identifies the <see cref="M11"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M11Property = DependencyProperty.Register("M11", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M12"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M12Property = DependencyProperty.Register("M12", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M13"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M13Property = DependencyProperty.Register("M13", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M14"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M14Property = DependencyProperty.Register("M14", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M21"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M21Property = DependencyProperty.Register("M21", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M22"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M22Property = DependencyProperty.Register("M22", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M23"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M23Property = DependencyProperty.Register("M23", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M24"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M24Property = DependencyProperty.Register("M24", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M31"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M31Property = DependencyProperty.Register("M31", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M32"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M32Property = DependencyProperty.Register("M32", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M33"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M33Property = DependencyProperty.Register("M33", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M34"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M34Property = DependencyProperty.Register("M34", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M41"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M41Property = DependencyProperty.Register("M41", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M42"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M42Property = DependencyProperty.Register("M42", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M43"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M43Property = DependencyProperty.Register("M43", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        /// <summary>
        /// Identifies the <see cref="M44"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty M44Property = DependencyProperty.Register("M44", typeof(float), typeof(MatrixEditor), new FrameworkPropertyMetadata(.0f, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnElementValuePropertyChanged));

        static MatrixEditor()
        {
            PropertyToIndex = new Dictionary<DependencyProperty, int> {
                { M11Property, 0 }, { M12Property, 1 }, { M13Property, 2 }, { M14Property, 3 },
                { M21Property, 4 }, { M22Property, 5 }, { M23Property, 6 }, { M24Property, 7 },
                { M31Property, 8 }, { M32Property, 9 }, { M33Property, 10 }, { M34Property, 11 },
                { M41Property, 12 }, { M42Property, 13 }, { M43Property, 14 }, { M44Property, 15 },
            };
        }
    
        /// <summary>
        /// The <see cref="Matrix"/> associated to this control.
        /// </summary>
        public Matrix Matrix { get { return (Matrix)GetValue(MatrixProperty); } set { SetValue(MatrixProperty, value); } }

        /// <summary>
        /// The value at the first column of the first row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M11 { get { return (float)GetValue(M11Property); } set { SetValue(M11Property, value); } }

        /// <summary>
        /// The value at the second column of the first row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M12 { get { return (float)GetValue(M12Property); } set { SetValue(M12Property, value); } }

        /// <summary>
        /// The value at the third column of the first row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M13 { get { return (float)GetValue(M13Property); } set { SetValue(M13Property, value); } }

        /// <summary>
        /// The value at the fourth column of the first row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M14 { get { return (float)GetValue(M14Property); } set { SetValue(M14Property, value); } }

        /// <summary>
        /// The value at the first column of the second row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M21 { get { return (float)GetValue(M21Property); } set { SetValue(M21Property, value); } }

        /// <summary>
        /// The value at the second column of the second row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M22 { get { return (float)GetValue(M22Property); } set { SetValue(M22Property, value); } }

        /// <summary>
        /// The value at the third column of the second row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M23 { get { return (float)GetValue(M23Property); } set { SetValue(M23Property, value); } }

        /// <summary>
        /// The value at the fourth column of the second row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M24 { get { return (float)GetValue(M24Property); } set { SetValue(M24Property, value); } }

        /// <summary>
        /// The value at the first column of the third row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M31 { get { return (float)GetValue(M31Property); } set { SetValue(M31Property, value); } }

        /// <summary>
        /// The value at the second column of the third row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M32 { get { return (float)GetValue(M32Property); } set { SetValue(M32Property, value); } }

        /// <summary>
        /// The value at the third column of the third row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M33 { get { return (float)GetValue(M33Property); } set { SetValue(M33Property, value); } }

        /// <summary>
        /// The value at the fourth column of the third row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M34 { get { return (float)GetValue(M34Property); } set { SetValue(M34Property, value); } }

        /// <summary>
        /// The value at the first column of the fourth row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M41 { get { return (float)GetValue(M41Property); } set { SetValue(M41Property, value); } }

        /// <summary>
        /// The value at the second column of the fourth row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M42 { get { return (float)GetValue(M42Property); } set { SetValue(M42Property, value); } }

        /// <summary>
        /// The value at the third column of the fourth row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M43 { get { return (float)GetValue(M43Property); } set { SetValue(M43Property, value); } }

        /// <summary>
        /// The value at the fourth column of the fourth row of the <see cref="Matrix"/> associated to this control.
        /// </summary>
        public float M44 { get { return (float)GetValue(M44Property); } set { SetValue(M44Property, value); } }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            templateApplied = false;
            base.OnApplyTemplate();
            templateApplied = true;
        }

        /// <summary>
        /// Raised when the <see cref="Matrix"/> property is modified.
        /// </summary>
        private void OnMatrixValueChanged()
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = MatrixProperty;

            if (!interlock)
            {
                interlock = true;
                foreach (var dependencyProperty in PropertyToIndex)
                {
                    SetCurrentValue(dependencyProperty.Key, Matrix[dependencyProperty.Value]);
                }
                interlock = false;
            }

            UpdateBinding(MatrixProperty);
            if (isInitializing)
                initializingProperty = null;
        }

        /// <summary>
        /// Raised when the one of the element properties is modified.
        /// </summary>
        /// <param name="e">The dependency property that has changed.</param>
        private void OnElementValueChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = e.Property;
            
            if (!interlock)
            {
                interlock = true;
                var array = new float[16];
                foreach (var dependencyProperty in PropertyToIndex)
                {
                    array[dependencyProperty.Value] = e.Property == dependencyProperty.Key ? (float)e.NewValue : Matrix[dependencyProperty.Value];
                }

                Matrix = new Matrix(array);
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
        /// Raised by <see cref="MatrixProperty"/> when the <see cref="Matrix"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnMatrixPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (MatrixEditor)sender;
            editor.OnMatrixValueChanged();
        }

        /// <summary>
        /// Raised by any element dependency property when it is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnElementValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (MatrixEditor)sender;
            editor.OnElementValueChanged(e);
        }
    }
}