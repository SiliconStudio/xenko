// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace SiliconStudio.Presentation.Quantum.ComponentModel
{
    /// <summary>
    /// A default implementation of <see cref="IObservableNodePropertyProvider"/> that will construct a <see cref="ObservableNodePropertyDescriptor"/>
    /// for each member of the <see cref="IObservableNode.Children"/> collection of the associated node. This implementation supports dynamic changes
    /// of the Children collection.
    /// </summary>
    public class DefaultObservableNodePropertyProvider : IObservableNodePropertyProvider
    {
        private readonly PropertyDescriptorCollection propertyDescriptorCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultObservableNodePropertyProvider"/> class.
        /// </summary>
        /// <param name="observableNode">The <see cref="IObservableNode"/> associated to this instance.</param>
        public DefaultObservableNodePropertyProvider(IObservableNode observableNode)
        {
            var childProperties = observableNode.Children.Select(x => new ObservableNodePropertyDescriptor(x));
            propertyDescriptorCollection = new PropertyDescriptorCollection(childProperties.Cast<PropertyDescriptor>().ToArray());
            var observableChildren = (ObservableCollection<IObservableNode>)observableNode.Children;
            observableChildren.CollectionChanged += ChildCollectionChanged;
        }

        /// <inheritdoc/>
        public PropertyDescriptorCollection GetProperties()
        {
            return propertyDescriptorCollection;
        }

        private void ChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                propertyDescriptorCollection.Clear();
            }

            if (e.NewItems != null)
            {
                int index = Math.Min(propertyDescriptorCollection.Count, e.NewStartingIndex >= 0 ? e.NewStartingIndex : int.MaxValue);
                foreach (var descriptor in e.NewItems.Cast<IObservableNode>().Select(x => new ObservableNodePropertyDescriptor(x)))
                {
                    propertyDescriptorCollection.Insert(index++, descriptor);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var descriptor in e.OldItems.Cast<IObservableNode>().Select(x => propertyDescriptorCollection.Cast<ObservableNodePropertyDescriptor>().First(y => y.Node == x)))
                {
                    propertyDescriptorCollection.Remove(descriptor);
                }
            }
        }
    }
}