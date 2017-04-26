// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Windows;

namespace SiliconStudio.Presentation.Core
{
    public class ValidationRoutedEventArgs : RoutedEventArgs
    {
        public object Value { get; }

        public ValidationRoutedEventArgs(RoutedEvent routedEvent, object value)
            : base(routedEvent)
        {
            Value = value;
        }
    }

    public class ValidationRoutedEventArgs<T> : ValidationRoutedEventArgs
    {
        public new T Value => (T)base.Value;

        public ValidationRoutedEventArgs(RoutedEvent routedEvent, T value)
            : base(routedEvent, value)
        {
        }
    }

    public delegate void ValidationRoutedEventHandler(object sender, ValidationRoutedEventArgs e);

    public delegate void ValidationRoutedEventHandler<T>(object sender, ValidationRoutedEventArgs<T> e);
}
