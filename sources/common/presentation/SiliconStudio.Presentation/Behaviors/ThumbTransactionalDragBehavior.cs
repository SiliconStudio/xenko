// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

using SiliconStudio.ActionStack;

namespace SiliconStudio.Presentation.Behaviors
{
    public class ThumbTransactionalDragBehavior : Behavior<Thumb>
    {
        public static DependencyProperty ActionStackProperty = DependencyProperty.Register("ActionStack", typeof(TransactionalActionStack), typeof(ThumbTransactionalDragBehavior));

        public TransactionalActionStack ActionStack { get { return (TransactionalActionStack)GetValue(ActionStackProperty); } set { SetValue(ActionStackProperty, value); } }

        protected override void OnAttached()
        {
            AssociatedObject.DragStarted += DragStarted;
            AssociatedObject.DragCompleted += DragCompleted;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.DragStarted -= DragStarted;
            AssociatedObject.DragCompleted -= DragCompleted;
        }

        protected virtual void DragStarted(object sender, DragStartedEventArgs e)
        {
            if (ActionStack != null)
            {
                ActionStack.BeginTransaction();
            }
        }

        protected virtual void DragCompleted(object sender, DragCompletedEventArgs e)
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