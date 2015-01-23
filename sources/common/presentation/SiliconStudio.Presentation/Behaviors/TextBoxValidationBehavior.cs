// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interactivity;

using SiliconStudio.Presentation.Core;
using SiliconStudio.Presentation.Extensions;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// This behavior allows to update the <see cref="Binding"/> of the <see cref="TextBox.Text"/> property both when the control losts focus and when the user press <b>Enter</b>.
    /// When the user press <b>Escape</b>, the <see cref="TextBox.Text"/> property can be roll-backed to the source value, cancelling the modification.
    /// Additionally, a command can be bound to be executed on validation.
    /// </summary>
    /// <remarks>
    /// This behavior will clone the <see cref="TextBox.Text"/> property <see cref="Binding"/> and sets the <see cref="Binding.UpdateSourceTrigger"/> property to
    /// <see cref="UpdateSourceTrigger.Explicit"/> if the current value is not already <see cref="UpdateSourceTrigger.Explicit"/>.
    /// </remarks>
    [Obsolete("This behavior is obsolete. Use the TextBox control instead.")]
    public class TextBoxValidationBehavior : Behavior<TextBox>
    {
        /// <summary>
        /// Identifies the <see cref="GetFocusOnLoad"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GetFocusOnLoadProperty = DependencyProperty.Register("GetFocusOnLoad", typeof(bool), typeof(TextBoxValidationBehavior), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="ValidateWithEnter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidateWithEnterProperty = DependencyProperty.Register("ValidateWithEnter", typeof(bool), typeof(TextBoxValidationBehavior), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="ValidateOnLostFocus"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidateOnLostFocusProperty = DependencyProperty.Register("ValidateOnLostFocus", typeof(bool), typeof(TextBoxValidationBehavior), new PropertyMetadata(true, LostFocusActionChanged));
        
        /// <summary>
        /// Identifies the <see cref="CancelWithEscape"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelWithEscapeProperty = DependencyProperty.Register("CancelWithEscape", typeof(bool), typeof(TextBoxValidationBehavior), new PropertyMetadata(true));
       
        /// <summary>
        /// Identifies the <see cref="CancelOnLostFocus"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelOnLostFocusProperty = DependencyProperty.Register("CancelOnLostFocus", typeof(bool), typeof(TextBoxValidationBehavior), new PropertyMetadata(false, LostFocusActionChanged));
       
        /// <summary>
        /// Identifies the <see cref="ValidateCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidateCommandProperty = DependencyProperty.Register("ValidateCommand", typeof(ICommand), typeof(TextBoxValidationBehavior));
      
        /// <summary>
        /// Identifies the <see cref="ValidateCommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValidateCommandParameterProprty = DependencyProperty.Register("ValidateCommandParameter", typeof(object), typeof(TextBoxValidationBehavior));

        /// <summary>
        /// Identifies the <see cref="CancelCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(TextBoxValidationBehavior));

        /// <summary>
        /// Identifies the <see cref="CancelCommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelCommandParameterProprty = DependencyProperty.Register("CancelCommandParameter", typeof(object), typeof(TextBoxValidationBehavior));

        /// <summary>
        /// Raised when the TextBox changes are being validating.
        /// </summary>
        public static readonly RoutedEvent ValidatingEvent = EventManager.RegisterRoutedEvent("Validating", RoutingStrategy.Bubble, typeof(CancelRoutedEventHandler), typeof(TextBoxValidationBehavior));

        /// <summary>
        /// Raised when the TextBox changes are validated.
        /// </summary>
        public static readonly RoutedEvent ValidatedEvent = EventManager.RegisterRoutedEvent("Validated", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextBoxValidationBehavior));

        /// <summary>
        /// Raised when the TextBox changes are cancelled.
        /// </summary>
        public static readonly RoutedEvent CancelledEvent = EventManager.RegisterRoutedEvent("Cancelled", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextBoxValidationBehavior));

        /// <summary>
        /// Gets or sets whether the associated text box should get keyboard focus when this behavior is attached.
        /// </summary>
        public bool GetFocusOnLoad { get { return (bool)GetValue(GetFocusOnLoadProperty); } set { SetValue(GetFocusOnLoadProperty, value); } }

        /// <summary>
        /// Gets or sets whether the validation should happen when the user press <b>Enter</b>.
        /// </summary>
        public bool ValidateWithEnter { get { return (bool)GetValue(ValidateWithEnterProperty); } set { SetValue(ValidateWithEnterProperty, value); } }

        /// <summary>
        /// Gets or sets whether the validation should happen when the control losts focus.
        /// </summary>
        public bool ValidateOnLostFocus { get { return (bool)GetValue(ValidateOnLostFocusProperty); } set { SetValue(ValidateOnLostFocusProperty, value); } }

        /// <summary>
        /// Gets or sets whether the cancellation should happen when the user press <b>Escape</b>.
        /// </summary>
        public bool CancelWithEscape { get { return (bool)GetValue(CancelWithEscapeProperty); } set { SetValue(CancelWithEscapeProperty, value); } }

        /// <summary>
        /// Gets or sets whether the cancellation should happen when the control losts focus.
        /// </summary>
        public bool CancelOnLostFocus { get { return (bool)GetValue(CancelOnLostFocusProperty); } set { SetValue(CancelOnLostFocusProperty, value); } }

        /// <summary>
        /// Gets or sets the command to execute when the validation occurs.
        /// </summary>
        public ICommand ValidateCommand { get { return (ICommand)GetValue(ValidateCommandProperty); } set { SetValue(ValidateCommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to execute when the validation occurs.
        /// </summary>
        public object ValidateCommandParameter { get { return GetValue(ValidateCommandParameterProprty); } set { SetValue(ValidateCommandParameterProprty, value); } }

        /// <summary>
        /// Gets or sets the command to execute when the cancellation occurs.
        /// </summary>
        public ICommand CancelCommand { get { return (ICommand)GetValue(CancelCommandProperty); } set { SetValue(CancelCommandProperty, value); } }

        /// <summary>
        /// Gets or sets the parameter of the command to execute when the cancellation occurs.
        /// </summary>
        public object CancelCommandParameter { get { return GetValue(CancelCommandParameterProprty); } set { SetValue(CancelCommandParameterProprty, value); } }

        /// <summary>
        /// Raised when the TextBox changes are being validating.
        /// </summary>
        public event RoutedEventHandler Validating { add { AssociatedObject.AddHandler(ValidatingEvent, value); } remove { AssociatedObject.RemoveHandler(ValidatingEvent, value); } }

        /// <summary>
        /// Raised when the TextBox changes are validated.
        /// </summary>
        public event RoutedEventHandler Validated { add { AssociatedObject.AddHandler(ValidatedEvent, value); } remove { AssociatedObject.RemoveHandler(ValidatedEvent, value); } }

        /// <summary>
        /// Raised when the TextBox changes are cancelled.
        /// </summary>
        public event RoutedEventHandler Cancelled { add { AssociatedObject.AddHandler(CancelledEvent, value); } remove { AssociatedObject.RemoveHandler(CancelledEvent, value); } }

        /// <inheritdoc/>
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += TextBoxLoaded;
            AssociatedObject.KeyDown += TextBoxKeyDown;
            AssociatedObject.LostFocus += TextBoxLostFocus;

            var textBinding = BindingOperations.GetBinding(AssociatedObject, TextBox.TextProperty);
            if (textBinding != null)
            {
                if (textBinding.UpdateSourceTrigger != UpdateSourceTrigger.Explicit)
                {
                    var newBinding = textBinding.CloneBinding(textBinding.Mode);
                    AssociatedObject.SetBinding(TextBox.TextProperty, newBinding);
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= TextBoxLoaded;
            AssociatedObject.LostFocus -= TextBoxLostFocus;
            AssociatedObject.KeyDown -= TextBoxKeyDown;
            base.OnDetaching();
        }

        private void TextBoxLoaded(object sender, RoutedEventArgs e)
        {
            if (GetFocusOnLoad)
            {
                Keyboard.Focus(AssociatedObject);
            }
        }
        
        private void TextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && ValidateWithEnter)
            {
                Validate();
            }
            if (e.Key == Key.Escape && CancelWithEscape)
            {
                Cancel();
            }
        }

        private void TextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            if (ValidateOnLostFocus)
            {
                Validate();
            }
            if (CancelOnLostFocus)
            {
                Cancel();
            }
        }

        private void ClearUndoStack()
        {
            var limit = AssociatedObject.UndoLimit;
            AssociatedObject.UndoLimit = 0;
            AssociatedObject.UndoLimit = limit;
        }

        private void Validate()
        {
            var cancelRoutedEventArgs = new CancelRoutedEventArgs(ValidatingEvent);
            AssociatedObject.RaiseEvent(cancelRoutedEventArgs);
            if (cancelRoutedEventArgs.Cancel)
            {
                return;
            }
            
            BindingExpression expression = AssociatedObject.GetBindingExpression(TextBox.TextProperty);
            if (expression != null)
                expression.UpdateSource();

            ClearUndoStack();

            AssociatedObject.RaiseEvent(new RoutedEventArgs(ValidatedEvent));
            if (ValidateCommand != null && ValidateCommand.CanExecute(ValidateCommandParameter))
                ValidateCommand.Execute(ValidateCommandParameter);
        }

        private void Cancel()
        {
            BindingExpression expression = AssociatedObject.GetBindingExpression(TextBox.TextProperty);
            if (expression != null)
                expression.UpdateTarget();

            ClearUndoStack();

            AssociatedObject.RaiseEvent(new RoutedEventArgs(CancelledEvent));
            
            if (CancelCommand != null && CancelCommand.CanExecute(CancelCommandParameter))
                CancelCommand.Execute(CancelCommandParameter);
        }

        private static void LostFocusActionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (TextBoxValidationBehavior)d;
            if (e.Property == ValidateOnLostFocusProperty)
            {
                behavior.SetCurrentValue(CancelOnLostFocusProperty, !(bool)e.NewValue);
            }
            if (e.Property == CancelOnLostFocusProperty)
            {
                behavior.SetCurrentValue(ValidateOnLostFocusProperty, !(bool)e.NewValue);
            }
        }
    }
}
