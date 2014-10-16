// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace SiliconStudio.BuildEngine.Monitor.View
{
    public class SelectLastAddedItemBehavior : Behavior<Selector>
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(SelectLastAddedItemBehavior), new PropertyMetadata(ItemsSourceChanged));

        public IEnumerable ItemsSource { get { return (IEnumerable)GetValue(ItemsSourceProperty); } set { SetValue(ItemsSourceProperty, value); } }

        private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (SelectLastAddedItemBehavior)d;
            var oldValue = (INotifyCollectionChanged)e.OldValue;
            var newValue = (INotifyCollectionChanged)e.NewValue;
            if (oldValue != null)
            {
                oldValue.CollectionChanged -= behavior.CollectionChanged;
            }
            if (newValue != null)
            {
                newValue.CollectionChanged += behavior.CollectionChanged;
            }
        }

        private INotifyCollectionChanged itemsSource;
        protected override void OnAttached()
        {
            base.OnAttached();
            itemsSource = (INotifyCollectionChanged)AssociatedObject.ItemsSource;
            if (itemsSource != null) 
                itemsSource.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
                AssociatedObject.SelectedItem = e.NewItems.Cast<object>().Last();
        }
    }
}
