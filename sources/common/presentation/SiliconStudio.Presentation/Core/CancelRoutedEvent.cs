// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Windows;

namespace SiliconStudio.Presentation.Core
{
    public class CancelRoutedEventArgs : RoutedEventArgs
    {
        public bool Cancel { get; set; }

        public CancelRoutedEventArgs(RoutedEvent routedEvent, bool cancel = false)
            : base(routedEvent)
        {
            Cancel = cancel;
        }
    }

    public delegate void CancelRoutedEventHandler(object sender, CancelRoutedEventArgs e);
}
