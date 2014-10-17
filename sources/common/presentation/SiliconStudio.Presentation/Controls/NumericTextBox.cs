// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Controls
{
    public class RepeatButtonPressedRoutedEventArgs : RoutedEventArgs
    {
        public RepeatButtonPressedRoutedEventArgs(NumericTextBox.RepeatButtons button, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Button = button;
        }

        public NumericTextBox.RepeatButtons Button { get; private set; }
    }

    /// <summary>
    /// A specialization of the <see cref="TextBox"/> control that can be used for numeric values.
    /// It contains a <see cref="Value"/> property that is updated on validation.
    /// </summary>
    public class NumericTextBox : TextBox
    {
        public enum RepeatButtons
        {
            IncreaseButton,
            DecreaseButton,
        }

        private RepeatButton increaseButton;
        private RepeatButton decreaseButton;
        private bool updatingValue;

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(double), typeof(NumericTextBox), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged, null, true, UpdateSourceTrigger.Explicit));

        /// <summary>
        /// Identifies the <see cref="DecimalPlaces"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register("DecimalPlaces", typeof(int), typeof(NumericTextBox), new FrameworkPropertyMetadata(-1, OnDecimalPlacesPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Minimum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register("Minimum", typeof(double), typeof(NumericTextBox), new FrameworkPropertyMetadata(double.MinValue, OnMinimumPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register("Maximum", typeof(double), typeof(NumericTextBox), new FrameworkPropertyMetadata(double.MaxValue, OnMaximumPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="ValueRatio"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueRatioProperty = DependencyProperty.Register("ValueRatio", typeof(double), typeof(NumericTextBox), new PropertyMetadata(default(double), ValueRatioChanged));

        /// <summary>
        /// Identifies the <see cref="LargeChange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register("LargeChange", typeof(double), typeof(NumericTextBox), new PropertyMetadata(10.0));

        /// <summary>
        /// Identifies the <see cref="SmallChange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register("SmallChange", typeof(double), typeof(NumericTextBox), new PropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="DisplayUpDownButtons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayUpDownButtonsProperty = DependencyProperty.Register("DisplayUpDownButtons", typeof(bool), typeof(NumericTextBox), new PropertyMetadata(true));
        
        /// <summary>
        /// Raised when the <see cref="Value"/> property has changed.
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(NumericTextBox));

        /// <summary>
        /// Raised when one of the repeat button is pressed.
        /// </summary>
        public static readonly RoutedEvent RepeatButtonPressedEvent = EventManager.RegisterRoutedEvent("RepeatButtonPressed", RoutingStrategy.Bubble, typeof(EventHandler<RepeatButtonPressedRoutedEventArgs>), typeof(NumericTextBox));

        /// <summary>
        /// Raised when one of the repeat button is released.
        /// </summary>
        public static readonly RoutedEvent RepeatButtonReleasedEvent = EventManager.RegisterRoutedEvent("RepeatButtonReleased", RoutingStrategy.Bubble, typeof(EventHandler<RepeatButtonPressedRoutedEventArgs>), typeof(NumericTextBox));

        /// <summary>
        /// Increases the current value with the value of the <see cref="LargeChange"/> property.
        /// </summary>
        public static RoutedCommand LargeIncreaseCommand { get; private set; }

        /// <summary>
        /// Increases the current value with the value of the <see cref="SmallChange"/> property.
        /// </summary>
        public static RoutedCommand SmallIncreaseCommand { get; private set; }

        /// <summary>
        /// Decreases the current value with the value of the <see cref="LargeChange"/> property.
        /// </summary>
        public static RoutedCommand LargeDecreaseCommand { get; private set; }

        /// <summary>
        /// Decreases the current value with the value of the <see cref="SmallChange"/> property.
        /// </summary>
        public static RoutedCommand SmallDecreaseCommand { get; private set; }

        static NumericTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericTextBox), new FrameworkPropertyMetadata(typeof(NumericTextBox)));

            // Since the NumericTextBox is not focusable itself, we have to bind the commands to the inner text box of the control.
            // The handlers will then find the parent that is a NumericTextBox and process the command on this control if it is found.
            LargeIncreaseCommand = new RoutedCommand("LargeIncreaseCommand", typeof(System.Windows.Controls.TextBox));
            CommandManager.RegisterClassCommandBinding(typeof(System.Windows.Controls.TextBox), new CommandBinding(LargeIncreaseCommand, OnLargeIncreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(System.Windows.Controls.TextBox), new InputBinding(LargeIncreaseCommand, new KeyGesture(Key.PageUp)));
            CommandManager.RegisterClassInputBinding(typeof(System.Windows.Controls.TextBox), new InputBinding(LargeIncreaseCommand, new KeyGesture(Key.Up, ModifierKeys.Shift)));

            LargeDecreaseCommand = new RoutedCommand("LargeDecreaseCommand", typeof(System.Windows.Controls.TextBox));
            CommandManager.RegisterClassCommandBinding(typeof(System.Windows.Controls.TextBox), new CommandBinding(LargeDecreaseCommand, OnLargeDecreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(System.Windows.Controls.TextBox), new InputBinding(LargeDecreaseCommand, new KeyGesture(Key.PageDown)));
            CommandManager.RegisterClassInputBinding(typeof(System.Windows.Controls.TextBox), new InputBinding(LargeDecreaseCommand, new KeyGesture(Key.Down, ModifierKeys.Shift)));

            SmallIncreaseCommand = new RoutedCommand("SmallIncreaseCommand", typeof(System.Windows.Controls.TextBox));
            CommandManager.RegisterClassCommandBinding(typeof(System.Windows.Controls.TextBox), new CommandBinding(SmallIncreaseCommand, OnSmallIncreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(System.Windows.Controls.TextBox), new InputBinding(SmallIncreaseCommand, new KeyGesture(Key.Up)));

            SmallDecreaseCommand = new RoutedCommand("SmallDecreaseCommand", typeof(System.Windows.Controls.TextBox));
            CommandManager.RegisterClassCommandBinding(typeof(System.Windows.Controls.TextBox), new CommandBinding(SmallDecreaseCommand, OnSmallDecreaseCommand));
            CommandManager.RegisterClassInputBinding(typeof(System.Windows.Controls.TextBox), new InputBinding(SmallDecreaseCommand, new KeyGesture(Key.Down)));
        }

        /// <summary>
        /// Gets or sets the current value of the <see cref="NumericTextBox"/>.
        /// </summary>
        public double Value { get { return (double)GetValue(ValueProperty); } set { SetValue(ValueProperty, value); } }

        /// <summary>
        /// Gets or sets the number of decimal places displayed in the <see cref="NumericTextBox"/>.
        /// </summary>
        public int DecimalPlaces { get { return (int)GetValue(DecimalPlacesProperty); } set { SetValue(DecimalPlacesProperty, value); } }

        /// <summary>
        /// Gets or sets the minimum value that can be set on the <see cref="Value"/> property.
        /// </summary>
        public double Minimum { get { return (double)GetValue(MinimumProperty); } set { SetValue(MinimumProperty, value); } }

        /// <summary>
        /// Gets or sets the maximum value that can be set on the <see cref="Value"/> property.
        /// </summary>
        public double Maximum { get { return (double)GetValue(MaximumProperty); } set { SetValue(MaximumProperty, value); } }

        /// <summary>
        /// Gets or sets the ratio of the <see cref="NumericTextBox.Value"/> between <see cref="NumericTextBox.Minimum"/> (0.0) and
        /// <see cref="NumericTextBox.Maximum"/> (1.0).
        /// </summary>
        public double ValueRatio { get { return (double)GetValue(ValueRatioProperty); } set { SetValue(ValueRatioProperty, value); } }

        /// <summary>
        /// Gets or sets the value to be added to or substracted from the <see cref="NumericTextBox.Value"/>.
        /// </summary>
        public double LargeChange { get { return (double)GetValue(LargeChangeProperty); } set { SetValue(LargeChangeProperty, value); } }

        /// <summary>
        /// Gets or sets the value to be added to or substracted from the <see cref="NumericTextBox.Value"/>.
        /// </summary>
        public double SmallChange { get { return (double)GetValue(SmallChangeProperty); } set { SetValue(SmallChangeProperty, value); } }

        /// <summary>
        /// Gets or sets whether to display Up and Down buttons on the side of the <see cref="NumericTextBox"/>.
        /// </summary>
        public bool DisplayUpDownButtons { get { return (bool)GetValue(DisplayUpDownButtonsProperty); } set { SetValue(DisplayUpDownButtonsProperty, value); } }
        
        /// <summary>
        /// Raised when the <see cref="Value"/> property has changed.
        /// </summary>
        public event RoutedPropertyChangedEventHandler<double> ValueChanged { add { AddHandler(ValueChangedEvent, value); } remove { RemoveHandler(ValueChangedEvent, value); } }

        /// <summary>
        /// Raised when one of the repeat button is pressed.
        /// </summary>
        public event EventHandler<RepeatButtonPressedRoutedEventArgs> RepeatButtonPressed { add { AddHandler(RepeatButtonPressedEvent, value); } remove { RemoveHandler(RepeatButtonPressedEvent, value); } }

        /// <summary>
        /// Raised when one of the repeat button is released.
        /// </summary>
        public event EventHandler<RepeatButtonPressedRoutedEventArgs> RepeatButtonReleased { add { AddHandler(RepeatButtonReleasedEvent, value); } remove { RemoveHandler(RepeatButtonReleasedEvent, value); } }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            increaseButton = GetTemplateChild("PART_IncreaseButton") as RepeatButton;
            if (increaseButton == null)
                throw new InvalidOperationException("A part named 'PART_IncreaseButton' must be present in the ControlTemplate, and must be of type 'RepeatButton'.");

            decreaseButton = GetTemplateChild("PART_DecreaseButton") as RepeatButton;
            if (decreaseButton == null)
                throw new InvalidOperationException("A part named 'PART_DecreaseButton' must be present in the ControlTemplate, and must be of type 'RepeatButton'.");

            var increasePressedWatcher = new DependencyPropertyWatcher(increaseButton);
            increasePressedWatcher.RegisterValueChangedHandler(ButtonBase.IsPressedProperty, RepeatButtonIsPressedChanged);
            var decreasePressedWatcher = new DependencyPropertyWatcher(decreaseButton);
            decreasePressedWatcher.RegisterValueChangedHandler(ButtonBase.IsPressedProperty, RepeatButtonIsPressedChanged);
            var textValue = FormatValue(Value);
            
            SetCurrentValue(TextProperty, textValue);
        }

        /// <summary>
        /// Raised when the <see cref="Value"/> property has changed.
        /// </summary>
        protected virtual void OnValueChanged(double oldValue, double newValue)
        {
        }

        /// <inheritdoc/>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            var textValue = FormatValue(Value);
            SetCurrentValue(TextProperty, textValue);
        }

        protected sealed override void OnCancelled()
        {
            var textValue = FormatValue(Value);
            SetCurrentValue(TextProperty, textValue);
        }

        /// <inheritdoc/>
        protected sealed override void OnValidated()
        {
            double value;
            try
            {
                try
                {
                    value = double.Parse(Text);
                }
                catch (Exception)
                {
                    value = double.Parse(Text, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception)
            {
                value = Value;
            }
            SetCurrentValue(ValueProperty, value);

            BindingExpression expression = GetBindingExpression(ValueProperty);
            if (expression != null)
                expression.UpdateSource();
        }

        /// <inheritdoc/>
        protected override string CoerceTextForValidation(string baseValue)
        {
            baseValue = base.CoerceTextForValidation(baseValue);
            double value;
            try
            {
                try
                {
                    value = double.Parse(baseValue);
                }
                catch (Exception)
                {
                    value = double.Parse(baseValue, CultureInfo.InvariantCulture);
                }
                if (value > Maximum)
                {
                    value = Maximum;
                }
                if (value < Minimum)
                {
                    value = Minimum;
                }
            }
            catch (Exception)
            {
                value = Value;
            }

            return FormatValue(value);
        }

        /// <summary>
        /// Formats the text to 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected string FormatValue(double value)
        {
            int decimalPlaces = DecimalPlaces;
            double coercedValue = decimalPlaces < 0 ? value : Math.Round(value, decimalPlaces);
            return coercedValue.ToString();
        }

        private void RepeatButtonIsPressedChanged(object sender, EventArgs e)
        {
            var repeatButton = (RepeatButton)sender;
            if (ReferenceEquals(repeatButton, increaseButton))
            {
                RaiseEvent(new RepeatButtonPressedRoutedEventArgs(RepeatButtons.IncreaseButton, repeatButton.IsPressed ? RepeatButtonPressedEvent : RepeatButtonReleasedEvent));
            }
            if (ReferenceEquals(repeatButton, decreaseButton))
            {
                RaiseEvent(new RepeatButtonPressedRoutedEventArgs(RepeatButtons.DecreaseButton, repeatButton.IsPressed ? RepeatButtonPressedEvent : RepeatButtonReleasedEvent));
            }
        }
        
        private void OnValuePropertyChanged(double oldValue, double newValue)
        {
            if (newValue > Maximum)
            {
                SetCurrentValue(ValueProperty, Maximum);
                return;
            }
            if (newValue < Minimum)
            {
                SetCurrentValue(ValueProperty, Minimum);
                return;
            }

            var textValue = FormatValue(newValue);
            updatingValue = true;
            SetCurrentValue(TextProperty, textValue);
            SetCurrentValue(ValueRatioProperty, MathUtil.InverseLerp(Minimum, Maximum, newValue));
            updatingValue = false;

            RaiseEvent(new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent));
            OnValueChanged(oldValue, newValue);
        }

        private void UpdateValue(double value)
        {
            if (IsReadOnly == false)
            {
                SetCurrentValue(ValueProperty, value);
            }
        }
        
        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((NumericTextBox)sender).OnValuePropertyChanged((double)e.OldValue, (double)e.NewValue);
        }
        
        private static void OnDecimalPlacesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var numericInput = (NumericTextBox)sender;
            numericInput.CoerceValue(ValueProperty);
        }

        private static void OnMinimumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var numericInput = (NumericTextBox)sender;
            bool needValidation = false;
            if (numericInput.Maximum < numericInput.Minimum)
            {
                numericInput.SetCurrentValue(MaximumProperty, numericInput.Minimum);
                needValidation = true;
            }
            if (numericInput.Value < numericInput.Minimum)
            {
                numericInput.SetCurrentValue(ValueProperty, numericInput.Minimum);
                needValidation = true;
            }

            // Do not overwrite the Value, it is already correct!
            numericInput.updatingValue = true;
            numericInput.SetCurrentValue(ValueRatioProperty, MathUtil.InverseLerp(numericInput.Minimum, numericInput.Maximum, numericInput.Value));
            numericInput.updatingValue = false;

            if (needValidation)
            {
                numericInput.Validate();
            }
        }

        private static void OnMaximumPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var numericInput = (NumericTextBox)sender;
            bool needValidation = false;
            if (numericInput.Minimum > numericInput.Maximum)
            {
                numericInput.SetCurrentValue(MinimumProperty, numericInput.Maximum);
                needValidation = true;
            }
            if (numericInput.Value > numericInput.Maximum)
            {
                numericInput.SetCurrentValue(ValueProperty, numericInput.Maximum);
                needValidation = true;
            }

            // Do not overwrite the Value, it is already correct!
            numericInput.updatingValue = true;
            numericInput.SetCurrentValue(ValueRatioProperty, MathUtil.InverseLerp(numericInput.Minimum, numericInput.Maximum, numericInput.Value));
            numericInput.updatingValue = false;
      
            if (needValidation)
            {
                numericInput.Validate();
            }
        }

        private static void ValueRatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericTextBox)d;
            if (control != null && !control.updatingValue)
                control.UpdateValue(MathUtil.Lerp(control.Minimum, control.Maximum, (double)e.NewValue));
        }

        private static void UpdateValueCommand(object sender, Func<NumericTextBox, double> getValue)
        {
            var control = (sender as NumericTextBox) ?? ((System.Windows.Controls.TextBox)sender).FindVisualParentOfType<NumericTextBox>();
            if (control != null)
            {
                var value = getValue(control);
                control.UpdateValue(value);
                control.SelectAll();
                control.Validate();
            }
        }

        private static void OnLargeIncreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateValueCommand(sender, x => x.Value + x.LargeChange);
        }

        private static void OnLargeDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateValueCommand(sender, x => x.Value - x.LargeChange);
        }

        private static void OnSmallIncreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateValueCommand(sender, x => x.Value + x.SmallChange);
        }

        private static void OnSmallDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateValueCommand(sender, x => x.Value - x.SmallChange);
        }

        // TODO: Constraint accepted characters? Legacy code that can help here:
        //public static readonly string NumberDecimalSeparator;
        //public static readonly string NumberGroupSeparator;
        //public static readonly string NegativeSign;

        // Previously in static constructor:
        //NumberFormatInfo numberFormatInfo = CultureInfo.CurrentCulture.NumberFormat;
        //NumberDecimalSeparator = numberFormatInfo.NumberDecimalSeparator;
        //NumberGroupSeparator = numberFormatInfo.NumberGroupSeparator;
        //NegativeSign = numberFormatInfo.NegativeSign;
        // -> But we should fetch these value each time we need them because the current culture can be changed at runtime!

        //protected override void OnPreviewKeyDown(KeyEventArgs e)
        //{
        //    if (IsInvalidCommon(e.Key))
        //    {
        //        e.Handled = true;
        //        return;
        //    }

        //    base.OnPreviewKeyDown(e);
        //}

        //protected virtual bool IsInvalidCommon(Key key)
        //{
        //    return false;
        //}
        
        //protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        //{
        //    base.OnPreviewTextInput(e);

        //    string str = e.Text;
        //    if (str == null || str.Length != 1)
        //    {
        //        e.Handled = true;
        //        return;
        //    }

        //    e.Handled = !IsValidStringElement(str);
        //}
        
        //protected virtual bool IsValidStringElement(string c)
        //{
        //    return IsValidFloatingPointStringElement(c);
        //}
        
        //public static bool IsValidFloatingPointStringElement(string c)
        //{
        //    if (c == NumberDecimalSeparator || c == NumberGroupSeparator || c == NegativeSign)
        //        return true;

        //    char cc = c[0];
        //    if (cc >= '0' && cc <= '9')
        //        return true;

        //    return false;
        //}

        //public static bool IsValidIntegerStringElement(string c)
        //{
        //    if (c == NumberGroupSeparator || c == NegativeSign)
        //        return true;

        //    char cc = c[0];
        //    if (cc >= '0' && cc <= '9')
        //        return true;

        //    return false;
        //}

        //public static bool IsValidHexadecimalStringElement(string c)
        //{
        //    char cc = c[0];
        //    if ((cc >= 'a' && cc <= 'f') || (cc >= 'A' && cc <= 'F'))
        //        return true;

        //    return false;
        //}
    }
}
