// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using SiliconStudio.ActionStack;
using SiliconStudio.Presentation.Controls;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// This behavior allows more convenient editing of the value of a char using a TextBox.
    /// </summary>
    public class NumericTextBoxTransactionalRepeatButtonsBehavior : Behavior<NumericTextBox>
    {
        public static DependencyProperty ActionStackProperty = DependencyProperty.Register("ActionStack", typeof(TransactionalActionStack), typeof(NumericTextBoxTransactionalRepeatButtonsBehavior));

        public TransactionalActionStack ActionStack { get { return (TransactionalActionStack)GetValue(ActionStackProperty); } set { SetValue(ActionStackProperty, value); } }

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
            ActionStack?.BeginTransaction();
        }

        private void RepeatButtonReleased(object sender, RepeatButtonPressedRoutedEventArgs e)
        {
            if (ActionStack != null)
            {
                var items = ActionStack.GetCurrentTransactions();
                if (items.Count > 0)
                {
                    // We use the name of the first action (all actions in this list are
                    // supposed to be the same (ie. with the same name)
                    var name = items.First().Name;
                    ActionStack.EndTransaction(name);
                }
                else
                {
                    // This is not supposed to happen
                    ActionStack.DiscardTransaction();
                }
            }
        }
    }
}
