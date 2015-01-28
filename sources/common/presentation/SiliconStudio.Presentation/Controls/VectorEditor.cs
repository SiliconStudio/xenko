using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SiliconStudio.Presentation.Controls
{
    public abstract class VectorEditor : Control
    {
        /// <summary>
        /// Sets the vector value of this vector editor from a single float value.
        /// </summary>
        /// <param name="value">The value to use to generate a vector.</param>
        public abstract void SetVectorFromValue(float value);
    }

    public abstract class VectorEditor<T> : VectorEditor
    {
        private bool interlock;
        private bool templateApplied;
        private DependencyProperty initializingProperty;

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(T), typeof(VectorEditor<T>), new FrameworkPropertyMetadata(default(T), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged, null, false, UpdateSourceTrigger.Explicit));

        /// <summary>
        /// The vector associated to this control.
        /// </summary>
        public T Value { get { return (T)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }

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
            var editor = (VectorEditor<T>)sender;
            editor.OnComponentPropertyChanged(e);
        }

        /// <summary>
        /// Raised by <see cref="ValueProperty"/> when the <see cref="Value"/> dependency property is modified.
        /// </summary>
        /// <param name="sender">The dependency object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var editor = (VectorEditor<T>)sender;
            editor.OnValueValueChanged();
        }
    }
}