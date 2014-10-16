// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace SiliconStudio.Presentation.Controls
{
    /// <summary>
    /// An input control that validates its <see cref="System.Windows.Controls.TextBox.Text"/> property after a certain amount of time since the last input.
    /// </summary>
    public class TimerTextBox : TextBox
    {
        private readonly Timer validationTimer;

        /// <summary>
        /// Identifies the <see cref="IsTimedValidationEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsTimedValidationEnabledProperty = DependencyProperty.Register("IsTimedValidationEnabled", typeof(bool), typeof(TimerTextBox), new PropertyMetadata(true, OnIsAutoSubmitEnabledPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="ValidationDelay"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidationDelayProperty = DependencyProperty.Register("ValidationDelay", typeof(int), typeof(TimerTextBox), new PropertyMetadata(500));

        /// <summary>
        /// Initializes the default style of the <see cref="TimerTextBox"/> control.
        /// </summary>
        static TimerTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimerTextBox), new FrameworkPropertyMetadata(typeof(TimerTextBox)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerTextBox"/> class.
        /// </summary>
        public TimerTextBox()
        {
            if (DesignerProperties.GetIsInDesignMode(this) == false)
                validationTimer = new Timer(x => Dispatcher.InvokeAsync(Validate), null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Gets or sets whether the text should be automatically validated after a delay defined by the <see cref="ValidationDelay"/> property.
        /// </summary>
        public bool IsTimedValidationEnabled { get { return (bool)GetValue(IsTimedValidationEnabledProperty); } set { SetValue(IsTimedValidationEnabledProperty, value); } }

        /// <summary>
        /// Gets or sets the amount of time before a validation of input text happens, in milliseconds.
        /// Every change to the <see cref="TextBox.Text"/> property reset the timer to this value.
        /// </summary>
        /// <remarks>The default value is <c>500</c> milliseconds.</remarks>
        public int ValidationDelay { get { return (int)GetValue(ValidationDelayProperty); } set { SetValue(ValidationDelayProperty, value); } }

        protected override void OnTextChanged(string oldValue, string newValue)
        {
            if (IsTimedValidationEnabled)
            {
                if (ValidationDelay > 0.0)
                {
                    if (validationTimer != null)
                        validationTimer.Change(ValidationDelay, Timeout.Infinite);
                }
                else
                {
                    Validate();
                }
            }
        }

        private static void OnIsAutoSubmitEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var txt = (TimerTextBox)sender;
            if ((bool)e.NewValue)
            {
                txt.Validate();
            }
        }
    }
}
