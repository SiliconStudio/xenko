// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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

        public static DependencyProperty UndoRedoServiceProperty = DependencyProperty.Register(nameof(UndoRedoService), typeof(IUndoRedoService), typeof(NumericTextBoxTransactionalRepeatButtonsBehavior));

        public IUndoRedoService UndoRedoService { get { return (IUndoRedoService)GetValue(UndoRedoServiceProperty); } set { SetValue(UndoRedoServiceProperty, value); } }

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
            transaction = UndoRedoService?.CreateTransaction();
        }

        private void RepeatButtonReleased(object sender, RepeatButtonPressedRoutedEventArgs e)
        {
            transaction?.Continue();
            transaction?.Complete();
            transaction = null;
        }
    }
}
