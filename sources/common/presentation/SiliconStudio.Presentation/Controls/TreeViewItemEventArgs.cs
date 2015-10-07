// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Windows;

namespace SiliconStudio.Presentation.Controls
{
    public class TreeViewItemEventArgs : RoutedEventArgs
    {
        public TreeViewItem Container { get; private set; }

        public object Item { get; private set; }

        public TreeViewItemEventArgs(RoutedEvent routedEvent, object source, TreeViewItem container, object item)
            : base(routedEvent, source)
        {
            Container = container;
            Item = item;
        }
    }
}