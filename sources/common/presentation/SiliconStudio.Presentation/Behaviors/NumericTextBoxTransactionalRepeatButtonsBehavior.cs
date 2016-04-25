// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Windows;
using System.Windows.Interactivity;
using SiliconStudio.Core.Transactions;
using SiliconStudio.Presentation.Controls;
using SiliconStudio.Presentation.Services;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// This behavior allows more convenient editing of the value of a char using a TextBox.
    /// </summary>
    public class NumericTextBoxTransactionalRepeatButtonsBehavior : Behavior<NumericTextBox>
    {
        private ITransaction transaction;

        public static DependencyProperty ActionServiceProperty = DependencyProperty.Register(nameof(ActionService), typeof(IUndoRedoService), typeof(NumericTextBoxTransactionalRepeatButtonsBehavior));

        public IUndoRedoService ActionService { get { return (IUndoRedoService)GetValue(ActionServiceProperty); } set { SetValue(ActionServiceProperty, value); } }

        protected override void OnAttached()
        {
            AssociatedObject.RepeatButtonPressed += RepeatButtonPressed;
            AssociatedObject.RepeatButtonReleased += RepeatButtonReleased;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.RepeatButtonPressed -= RepeatButtonPressed;
            AssociatedObject.RepeatButtonReleased -= RepeatButtonReleased;
        }

        private void RepeatButtonPressed(object sender, RepeatButtonPressedRoutedEventArgs e)
        {
            transaction = ActionService?.CreateTransaction();
        }

        private void RepeatButtonReleased(object sender, RepeatButtonPressedRoutedEventArgs e)
        {
            transaction?.Continue();
            transaction?.Complete();
            transaction = null;
        }
    }
}
