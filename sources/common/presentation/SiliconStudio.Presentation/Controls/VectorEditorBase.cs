// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SiliconStudio.Presentation.Controls
{
    public abstract class VectorEditorBase : Control
    {
        /// <summary>
        /// Identifies the <see cref="DecimalPlaces"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(VectorEditorBase), new FrameworkPropertyMetadata(-1));

        /// <summary>
        /// Identifies the <see cref="IsDropDownOpen"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(VectorEditorBase), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the number of decimal places displayed in the <see cref="NumericTextBox"/>.
        /// </summary>
        public int DecimalPlaces { get { return (int)GetValue(DecimalPlacesProperty); } set { SetValue(DecimalPlacesProperty, value); } }

        /// <summary>
        /// Gets or sets whether the drop-down of this vector editor is currently open
        /// </summary>
        public bool IsDropDownOpen { get { return (bool)GetValue(IsDropDownOpenProperty); } set { SetValue(IsDropDownOpenProperty, value); } }

        /// <summary>
        /// Sets the vector value of this vector editor from a single float value.
        /// </summary>
        /// <param name="value">The value to use to generate a vector.</param>
        public abstract void SetVectorFromValue(float value);

        public abstract void ResetValue();

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);
            if (IsDropDownOpen && !IsKeyboardFocusWithin)
            {
                SetCurrentValue(IsDropDownOpenProperty, false);
            }
        }
    }

    public abstract class VectorEditorBase<T> : VectorEditorBase
    {
        private bool interlock;
        private bool templateApplied;
        private DependencyProperty initializingProperty;

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(T), typeof(VectorEditorBase<T>), new FrameworkPropertyMetadata(default(T), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged, null, false, UpdateSourceTrigger.Explicit));

        /// <summary>
        /// Identifies the <see cref="DefaultValue"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(T), typeof(VectorEditorBase<T>), new PropertyMetadata(default(T)));

        /// <summary>
        /// Gets or sets the vector associated to this control.
        /// </summary>
        public T Value { get { return (T)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }

        /// <summary>
        /// Gets or sets the value that will be used by the <see cref="VectorEditorBase.ResetValue"/> method to reset the <see cref="Value"/> of this control.
        /// </summary>
        public T DefaultValue { get { return (T)GetValue(DefaultValueProperty); } set { SetValue(DefaultValueProperty, value); } }
        
        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            templateApplied = false;
            base.OnApplyTemplate();
            templateApplied = true;
        }

        /// <inheritdoc/>
        public override void SetVectorFromValue(float value)
        {
            Value = UpateValueFromFloat(value);
        }

        /// <inheritdoc/>
        public override void ResetValue()
        {
            Value = DefaultValue;
        }

        /// <summary>
        /// Updates the properties corresponding to the components of the vector from the given vector value.
        /// </summary>
        /// <param name="value">The vector from which to update component properties.</param>
        protected abstract void UpdateComponentsFromValue(T value);

        /// <summary>
        /// Updates the <see cref="Value"/> property according to a change in the given component property.
        /// </summary>
        /// <param name="property">The component property from which to update the <see cref="Value"/>.</param>
        protected abstract T UpdateValueFromComponent(DependencyProperty property);

        /// <summary>
        /// Updates the <see cref="Value"/> property from a single float.
        /// </summary>
        /// <param name="value">The value to use to generate a vector.</param>
        protected abstract T UpateValueFromFloat(float value);

        /// <summary>
        /// Raised when the <see cref="Value"/> property is modified.
        /// </summary>
        private void OnValueValueChanged()
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = ValueProperty;

            if (!interlock)
            {
                interlock = true;
                UpdateComponentsFromValue(Value);
                interlock = false;
            }

            UpdateBinding(ValueProperty);
            if (isInitializing)
                initializingProperty = null;
        }

        private void OnComponentPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            bool isInitializing = !templateApplied && initializingProperty == null;
            if (isInitializing)
                initializingProperty = e.Property;

            if (!interlock)
            {
                interlock = true;
                Value = UpdateValueFromComponent(e.Property);
                UpdateComponentsFromValue(Value);
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

        protected static void OnComponentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (VectorEditorBase<T>)sender;
            editor.OnComponentPropertyChanged(e);
        }

        protected static object CoerceComponentValue(DependencyObject sender, object basevalue)
        {
            var editor = (VectorEditorBase<T>)sender;
            int decimalPlaces = editor.DecimalPlaces;
            return decimalPlaces < 0 ? basevalue : (float)Math.Round((float)basevalue, decimalPlaces);
        }

        /// <summary>
        /// Raised by <see cref="ValueProperty"/> when the <see cref="Value"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (VectorEditorBase<T>)sender;
            editor.OnValueValueChanged();
        }
    }
}