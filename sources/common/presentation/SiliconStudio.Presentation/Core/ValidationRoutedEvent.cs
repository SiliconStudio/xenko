// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
