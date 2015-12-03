using System.Collections.Specialized;
using System.Windows;
using System.Windows.Interactivity;

namespace SiliconStudio.Presentation.Behaviors
{
    /// <summary>
    /// The base class for a behavior that allows to activate the associated object when an observable collection changes.
    /// </summary>
    /// <typeparam name="T">The type the <see cref="ActivateOnCollectionChangedBehavior{T}"/> can be attached to.</typeparam>
    public abstract class ActivateOnCollectionChangedBehavior<T> : Behavior<T> where T : DependencyObject
    {
        /// <summary>
        /// Identifies the <see cref="Collection"/> dependency property.
        /// </summary>
        public static DependencyProperty CollectionProperty = DependencyProperty.Register("Collection", typeof(INotifyCollectionChanged),
            typeof(ActivateOnCollectionChangedBehavior<T>), new FrameworkPropertyMetadata(OnCollectionChanged));

        /// <summary>
        /// Gets or sets the collection to observe in order to trigger activation of the associated control.
        /// </summary>
        public INotifyCollectionChanged Collection
        {
            get { return (INotifyCollectionChanged)GetValue(CollectionProperty); }
            set { SetValue(CollectionProperty, value); }
        }

        private static void OnCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (ActivateOnCollectionChangedBehavior<T>)d;
            if (e.OldValue != null)
            {
                var oldValue = (INotifyCollectionChanged)e.OldValue;
                oldValue.CollectionChanged -= behavior.CollectionChanged;
            }
            if (e.NewValue != null)
            {
                var newValue = (INotifyCollectionChanged)e.NewValue;
                newValue.CollectionChanged += behavior.CollectionChanged;
            }
        }

        /// <summary>
        /// Activates the associated object.
        /// </summary>
        protected abstract void Activate();

        protected virtual bool MatchChange(NotifyCollectionChangedEventArgs e)
        {
            return true;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MatchChange(e))
            {
                Activate();
            }
        }
    }
}
