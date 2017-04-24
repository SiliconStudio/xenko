// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Windows;

namespace SiliconStudio.Presentation.Controls
{
    public class PropertyViewItemEventArgs : RoutedEventArgs
    {
        public PropertyViewItem Container { get; private set; }

        public object Item { get; private set; }

        public PropertyViewItemEventArgs(RoutedEvent routedEvent, object source, PropertyViewItem container, object item)
            : base(routedEvent, source)
        {
            Container = container;
            Item = item;
        }
    }
}
