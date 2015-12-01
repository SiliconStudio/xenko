// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// A <see cref="Behavior{T}"/> that support deferred attachement for a FrameworkElement derived class.
    /// In such a case, the attachement is delayed until the OnLoad event is raised.
    /// </summary>
    /// <typeparam name="T">The type of instance to attach to.</typeparam>
    public abstract class DeferredBehaviorBase<T> : Behavior<T> where T : DependencyObject
    {
        private bool isClean;

        protected sealed override void OnAttached()
        {
            base.OnAttached();

            isClean = false;
            var element = AssociatedObject as FrameworkElement;

            if (element == null)
                OnAttachedOverride();
            else
            {
                if (element.IsLoaded)
                    OnAttachedOverride();
                else
                    element.Loaded += OnAssociatedObjectLoaded;

                element.Unloaded += OnAssociatedObjectUnloaded;
            }
        }

        protected sealed override void OnDetaching()
        {
            base.OnDetaching();

            CleanUp();
        }

        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            OnAttachedOverride();
            ((FrameworkElement)sender).Loaded -= OnAssociatedObjectLoaded;
        }

        private void OnAssociatedObjectUnloaded(object sender, RoutedEventArgs e)
        {
            CleanUp();
        }

        private void CleanUp()
        {
            if (isClean)
                return;

            isClean = true;
            
            var element = AssociatedObject as FrameworkElement;
            if (element != null)
            {
                element.Loaded -= OnAssociatedObjectLoaded;
                element.Unloaded -= OnAssociatedObjectUnloaded;
            }

            OnDetachingOverride();
        }

        protected virtual void OnAttachedOverride()
        {

        }

        protected virtual void OnDetachingOverride()
        {

        }
    }
}
