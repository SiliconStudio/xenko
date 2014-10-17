// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
