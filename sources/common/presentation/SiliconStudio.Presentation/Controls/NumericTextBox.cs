// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.Extensions;

using Point = System.Windows.Point;

namespace SiliconStudio.Presentation.Controls
{
    /// <summary>
    /// An enum describing when the related <see cref="NumericTextBox"/> should be validated, when the user uses the mouse to change its value.
    /// </summary>
    public enum MouseValidationTrigger
    {
        /// <summary>
        /// The validation occurs every time the mouse moves.
        /// </summary>
        OnMouseMove,
        /// <summary>
        /// The validation occurs when the mouse button is released.
        /// </summary>
        OnMouseUp,
    }

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
    /// A specialization of the <see cref="TextBoxBase"/> control that can be used for numeric values.
    /// It contains a <see cref="Value"/> property that is updated on validation.
    /// </summary>
    /// PART_IncreaseButton") as RepeatButton;
    [TemplatePart(Name = "PART_IncreaseButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_DecreaseButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_ContentHost", Type = typeof(ScrollViewer))]
    public class NumericTextBox : TextBoxBase
    {
        public enum RepeatButtons
        {
            IncreaseButton,
            DecreaseButton,
        }

        private enum DragState
        {
            None,
            Starting,
            Dragging,
        }

        private DragState dragState;
        private double mouseMoveDelta;
        private Point mouseDownPosition;
        private DragDirectionAdorner adorner;
        private Orientation dragOrientation;
        private RepeatButton increaseButton;
        private RepeatButton decreaseButton;
        private ScrollViewer contentHost;
        private bool updatingValue;

        /// <summary>
        /// The amount of pixel to move the mouse in order to add/remove a <see cref="SmallChange"/> to the current <see cref="Value"/>.
        /// </summary>
        public static double DragSpeed = 3;

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericTextBox), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValuePropertyChanged, null, true, UpdateSourceTrigger.Explicit));

        /// <summary>
        /// Identifies the <see cref="DecimalPlaces"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(NumericTextBox), new FrameworkPropertyMetadata(-1, OnDecimalPlacesPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Minimum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumericTextBox), new FrameworkPropertyMetadata(double.MinValue, OnMinimumPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Maximum"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumericTextBox), new FrameworkPropertyMetadata(double.MaxValue, OnMaximumPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="ValueRatio"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueRatioProperty = DependencyProperty.Register(nameof(ValueRatio), typeof(double), typeof(NumericTextBox), new PropertyMetadata(default(double), ValueRatioChanged));

        /// <summary>
        /// Identifies the <see cref="LargeChange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LargeChangeProperty = DependencyProperty.Register(nameof(LargeChange), typeof(double), typeof(NumericTextBox), new PropertyMetadata(10.0));

        /// <summary>
        /// Identifies the <see cref="SmallChange"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SmallChangeProperty = DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(NumericTextBox), new PropertyMetadata(1.0));

        /// <summary>
        /// Identifies the <see cref="DisplayUpDownButtons"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DisplayUpDownButtonsProperty = DependencyProperty.Register(nameof(DisplayUpDownButtons), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="AllowMouseDrag"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowMouseDragProperty = DependencyProperty.Register(nameof(AllowMouseDrag), typeof(bool), typeof(NumericTextBox), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="MouseValidationTrigger"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MouseValidationTriggerProperty = DependencyProperty.Register(nameof(MouseValidationTrigger), typeof(MouseValidationTrigger), typeof(NumericTextBox), new PropertyMetadata(MouseValidationTrigger.OnMouseUp));

        /// <summary>
        /// Identifies the <see cref="MouseValidationTrigger"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DragCursorProperty = DependencyProperty.Register(nameof(DragCursor), typeof(Cursor), typeof(NumericTextBox), new PropertyMetadata(Cursors.ScrollAll));
        
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
        public static RoutedCommand LargeIncreaseCommand { get; }

        /// <summary>
        /// Increases the current value with the value of the <see cref="SmallChange"/> property.
        /// </summary>
        public static RoutedCommand SmallIncreaseCommand { get; }

        /// <summary>
        /// Decreases the current value with the value of the <see cref="LargeChange"/> property.
        /// </summary>
        public static RoutedCommand LargeDecreaseCommand { get; }

        /// <summary>
        /// Decreases the current value with the value of the <see cref="SmallChange"/> property.
        /// </summary>
        public static RoutedCommand SmallDecreaseCommand { get; }

        /// <summary>
        /// Resets the current value to zero.
        /// </summary>
        public static RoutedCommand ResetValueCommand { get; }

        static NumericTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericTextBox), new FrameworkPropertyMetadata(typeof(NumericTextBox)));
            HorizontalScrollBarVisibilityProperty.OverrideMetadata(typeof(NumericTextBox), new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, OnForbiddenPropertyChanged));
            VerticalScrollBarVisibilityProperty.OverrideMetadata(typeof(NumericTextBox), new FrameworkPropertyMetadata(ScrollBarVisibility.Hidden, OnForbiddenPropertyChanged));
            AcceptsReturnProperty.OverrideMetadata(typeof(NumericTextBox), new FrameworkPropertyMetadata(false, OnForbiddenPropertyChanged));
            AcceptsTabProperty.OverrideMetadata(typeof(NumericTextBox), new FrameworkPropertyMetadata(false, OnForbiddenPropertyChanged));

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

            ResetValueCommand = new RoutedCommand("ResetValueCommand", typeof(System.Windows.Controls.TextBox));
            CommandManager.RegisterClassCommandBinding(typeof(System.Windows.Controls.TextBox), new CommandBinding(ResetValueCommand, OnResetValueCommand));
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
        /// Gets or sets whether dragging the value of the <see cref="NumericTextBox"/> is enabled.
        /// </summary>
        public bool AllowMouseDrag { get { return (bool)GetValue(AllowMouseDragProperty); } set { SetValue(AllowMouseDragProperty, value); } }

        /// <summary>
        /// Gets or sets the <see cref="Cursor"/> to display when the value can be modified via dragging.
        /// </summary>
        public Cursor DragCursor { get { return (Cursor)GetValue(DragCursorProperty); } set { SetValue(DragCursorProperty, value); } }

        /// <summary>
        /// Gets or sets when the <see cref="NumericTextBox"/> should be validated when the user uses the mouse to change its value.
        /// </summary>
        public MouseValidationTrigger MouseValidationTrigger { get { return (MouseValidationTrigger)GetValue(MouseValidationTriggerProperty); } set { SetValue(MouseValidationTriggerProperty, value); } }
        
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

        /// <summary>
        /// Raised when the mouse starts to drag the cursor to change the value.
        /// </summary>
        public event EventHandler<DragStartedEventArgs> DragStarted;

        /// <summary>
        /// Raised when the mouse stops to drag the cursor to change the value, after the validation of the change.
        /// </summary>
        public event EventHandler<DragCompletedEventArgs> DragCompleted;

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

            contentHost = GetTemplateChild("PART_ContentHost") as ScrollViewer;
            if (contentHost == null)
                throw new InvalidOperationException("A part named 'PART_ContentHost' must be present in the ControlTemplate, and must be of type 'ScrollViewer'.");

            var increasePressedWatcher = new DependencyPropertyWatcher(increaseButton);
            increasePressedWatcher.RegisterValueChangedHandler(ButtonBase.IsPressedProperty, RepeatButtonIsPressedChanged);
            var decreasePressedWatcher = new DependencyPropertyWatcher(decreaseButton);
            decreasePressedWatcher.RegisterValueChangedHandler(ButtonBase.IsPressedProperty, RepeatButtonIsPressedChanged);
            var textValue = FormatValue(Value);

            SetCurrentValue(TextProperty, textValue);

            contentHost.QueryCursor += HostQueryCursor;
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

        private void RootParentIsKeyboardFocusWithinChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue == false)
            {
                // Cancel dragging in progress
                if (dragState == DragState.Dragging)
                {
                    if (adorner != null)
                    {
                        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                        if (adornerLayer != null)
                        {
                            adornerLayer.Remove(adorner);
                            adorner = null;
                        }
                    }
                    Cancel();

                    var handler = DragCompleted;
                    if (handler != null)
                    {
                        var position = Mouse.GetPosition(this);
                        var dx = Math.Abs(position.X - mouseDownPosition.X);
                        var dy = Math.Abs(position.Y - mouseDownPosition.Y);
                        handler(this, new DragCompletedEventArgs(dx, dy, true));
                    }

                    Mouse.OverrideCursor = null;
                    dragState = DragState.None;

                }

                var root = this.FindVisualRoot() as FrameworkElement;
                if (root != null)
                    root.IsKeyboardFocusWithinChanged -= RootParentIsKeyboardFocusWithinChanged;
            }
        }

        /// <inheritdoc/>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);

            if (!IsContentHostPart(e.OriginalSource))
                return;

            if (AllowMouseDrag && IsReadOnly == false && IsFocused == false)
            {
                dragState = DragState.Starting;

                if (adorner == null)
                {
                    adorner = new DragDirectionAdorner(this, contentHost.ActualWidth);
                    var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                    adornerLayer?.Add(adorner);
                }

                mouseDownPosition = e.GetPosition(this);

                Mouse.OverrideCursor = Cursors.None;
                e.Handled = true;
            }
        }


        /// <inheritdoc/>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            var position = e.GetPosition(this);

            if (AllowMouseDrag && dragState == DragState.Starting && e.LeftButton == MouseButtonState.Pressed)
            {
                var dx = Math.Abs(position.X - mouseDownPosition.X);
                var dy = Math.Abs(position.Y - mouseDownPosition.Y);
                dragOrientation = dx >= dy ? Orientation.Horizontal : Orientation.Vertical;

                if (dx > SystemParameters.MinimumHorizontalDragDistance || dy > SystemParameters.MinimumVerticalDragDistance)
                {
                    var root = this.FindVisualRoot() as FrameworkElement;
                    if (root != null)
                        root.IsKeyboardFocusWithinChanged += RootParentIsKeyboardFocusWithinChanged;

                    mouseDownPosition = position;
                    mouseMoveDelta = 0;
                    dragState = DragState.Dragging;

                    e.MouseDevice.Capture(this);
                    DragStarted?.Invoke(this, new DragStartedEventArgs(mouseDownPosition.X, mouseDownPosition.Y));
                    SelectAll();
                    adorner?.SetOrientation(dragOrientation);
                }
            }

            if (dragState == DragState.Dragging)
            {
                if (dragOrientation == Orientation.Horizontal)
                    mouseMoveDelta += position.X - mouseDownPosition.X;
                else
                    mouseMoveDelta += mouseDownPosition.Y - position.Y;

                var deltaUsed = Math.Floor(mouseMoveDelta / DragSpeed);
                mouseMoveDelta -= deltaUsed;
                var newValue = Value + deltaUsed * SmallChange;

                SetCurrentValue(ValueProperty, newValue);

                if (MouseValidationTrigger == MouseValidationTrigger.OnMouseMove)
                {
                    Validate();
                }
                NativeHelper.SetCursorPos(PointToScreen(mouseDownPosition));
            }
        }

        /// <inheritdoc/>
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            if (ReferenceEquals(e.MouseDevice.Captured, this))
                e.MouseDevice.Capture(null);

            if (dragState == DragState.Starting)
            {
                Select(0, Text.Length);
                if (!IsFocused)
                {
                    Keyboard.Focus(this);
                }
            }
            else if (dragState == DragState.Dragging && AllowMouseDrag)
            {
                if (adorner != null)
                {
                    var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                    if (adornerLayer != null)
                    {
                        adornerLayer.Remove(adorner);
                        adorner = null;
                    }
                }
                Validate();

                var handler = DragCompleted;
                if (handler != null)
                {
                    var position = e.GetPosition(this);
                    var dx = Math.Abs(position.X - mouseDownPosition.X);
                    var dy = Math.Abs(position.Y - mouseDownPosition.Y);
                    handler(this, new DragCompletedEventArgs(dx, dy, false));
                }
            }

            Mouse.OverrideCursor = null;
            dragState = DragState.None;
        }

        protected sealed override void OnCancelled()
        {
            var expression = GetBindingExpression(ValueProperty);
            expression?.UpdateTarget();

            var textValue = FormatValue(Value);
            SetCurrentValue(TextProperty, textValue);
        }

        /// <inheritdoc/>
        protected sealed override void OnValidated()
        {
            double value;
            try
            {
                value = double.Parse(Text, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                value = Value;
            }
            SetCurrentValue(ValueProperty, value);

            var expression = GetBindingExpression(ValueProperty);
            expression?.UpdateSource();
        }

        /// <inheritdoc/>
        protected override string CoerceTextForValidation(string baseValue)
        {
            baseValue = base.CoerceTextForValidation(baseValue);
            double value;
            try
            {
                value = double.Parse(baseValue, CultureInfo.InvariantCulture);

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

        protected string FormatValue(double value)
        {
            var decimalPlaces = DecimalPlaces;
            var coercedValue = decimalPlaces < 0 ? value : Math.Round(value, decimalPlaces);
            return coercedValue.ToString(CultureInfo.InvariantCulture);
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

        private void HostQueryCursor(object sender, QueryCursorEventArgs e)
        {
            if (!IsContentHostPart(e.OriginalSource))
                return;

            if (AllowMouseDrag && !IsFocused && DragCursor != null)
            {
                e.Cursor = DragCursor;
                e.Handled = true;
            }
        }

        private bool IsContentHostPart(object obj)
        {
            var frameworkElement = obj as FrameworkElement;
            return Equals(obj, contentHost) || (frameworkElement != null && Equals(frameworkElement.Parent, contentHost));
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
            var needValidation = false;
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
            var needValidation = false;
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

        private static void UpdateValueCommand(object sender, Func<NumericTextBox, double> getValue, bool validate = true)
        {
            var control = (sender as NumericTextBox) ?? ((System.Windows.Controls.TextBox)sender).FindVisualParentOfType<NumericTextBox>();
            if (control != null)
            {
                var value = getValue(control);
                control.UpdateValue(value);
                control.SelectAll();
                if (validate)
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

        private static void OnResetValueCommand(object sender, ExecutedRoutedEventArgs e)
        {
            UpdateValueCommand(sender, x => 0.0, false);
        }

        private static void OnForbiddenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var metadata = e.Property.GetMetadata(d);
            if (!Equals(e.NewValue, metadata.DefaultValue))
            {
                var message = $"The value of the property '{e.Property.Name}' cannot be different from the value '{metadata.DefaultValue}'";
                throw new InvalidOperationException(message);
            }
        }

        private class DragDirectionAdorner : Adorner
        {
            private readonly double contentWidth;
            private static readonly ImageSource CursorHorizontalImageSource;
            private static readonly ImageSource CursorVerticalImageSource;

            static DragDirectionAdorner()
            {
                var asmName = Assembly.GetExecutingAssembly().GetName().Name;
                CursorHorizontalImageSource = ImageExtensions.ImageSourceFromFile($"pack://application:,,,/{asmName};component/Resources/Images/cursor_west_east.png");
                CursorVerticalImageSource = ImageExtensions.ImageSourceFromFile($"pack://application:,,,/{asmName};component/Resources/Images/cursor_north_south.png");
            }

            private Orientation dragOrientation;
            private bool ready;

            internal DragDirectionAdorner(UIElement adornedElement, double contentWidth)
                : base(adornedElement)
            {
                this.contentWidth = contentWidth;
            }

            internal void SetOrientation(Orientation orientation)
            {
                ready = true;
                dragOrientation = orientation;
                InvalidateVisual();
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (ready == false)
                    return;

                VisualEdgeMode = EdgeMode.Aliased;
                var source = dragOrientation == Orientation.Horizontal ? CursorHorizontalImageSource : CursorVerticalImageSource;
                var left = Math.Round(contentWidth - source.Width);
                var top = Math.Round((AdornedElement.RenderSize.Height - source.Height) * 0.5);
                drawingContext.DrawImage(source, new Rect(new Point(left, top), new Size(source.Width, source.Height)));
            }
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
