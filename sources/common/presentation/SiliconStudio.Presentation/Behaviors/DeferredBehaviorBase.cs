// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// A <see cref="Behavior{T}"/> that support deferred attachement for a FrameworkElement derived class.
    /// In such a case, the attachement is delayed until the <see cref="FrameworkElement.Loaded"/> event is raised.
    /// </summary>
    /// <typeparam name="T">The type of instance to attach to.</typeparam>
    public abstract class DeferredBehaviorBase<T> : Behavior<T> where T : DependencyObject
    {
        /// <summary>
        /// Represents the <see cref="AttachOnEveryLoadedEvent"/> property.
        /// </summary>
        public static readonly DependencyProperty AttachOnEveryLoadedEventProperty =
            DependencyProperty.Register(nameof(AttachOnEveryLoadedEvent), typeof(bool), typeof(DeferredBehaviorBase<T>), new PropertyMetadata(false));

        private bool isClean;

        /// <summary>
        /// Gets or sets whether <see cref="OnAttachedOverride"/> should be called each time the <see cref="FrameworkElement.Loaded"/> event is raised.
        /// </summary>
        public bool AttachOnEveryLoadedEvent { get { return (bool)GetValue(AttachOnEveryLoadedEventProperty); } set { SetValue(AttachOnEveryLoadedEventProperty, value); } }

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
                {
                    OnAttachedOverride();
                    if (AttachOnEveryLoadedEvent)
                        element.Loaded += OnAssociatedObjectLoaded;
                }
                else
                    element.Loaded += OnAssociatedObjectLoaded;

                element.Unloaded += OnAssociatedObjectUnloaded;
            }
        }

        protected sealed override void OnDetaching()
        {
            base.OnDetaching();

            CleanUp(true);
        }

        private void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e)
        {
            OnAttachedOverride();
            // HACK:
            // In some cases (e.g. in Telerik panes), the loaded event is called multiple times, without
            // Unloaded event in between. This might cause some behavior to not work properly. In such a case
            // set <see cref="AttachOnEveryLoadedEvent"/> to true.
            if (!AttachOnEveryLoadedEvent)
            {
                ((FrameworkElement)sender).Loaded -= OnAssociatedObjectLoaded;
            }
        }

        private void OnAssociatedObjectUnloaded(object sender, RoutedEventArgs e)
        {
            CleanUp(false);
        }

        private void CleanUp(bool isDetaching)
        {
            if (isClean)
                return;

            isClean = true;

            var element = AssociatedObject as FrameworkElement;
            if (element != null)
            {
                if (isDetaching)
                    element.Loaded -= OnAssociatedObjectLoaded;
                // see HACK in OnAssociatedObjectLoaded
                else if (!AttachOnEveryLoadedEvent)
                    element.Loaded += OnAssociatedObjectLoaded;
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
