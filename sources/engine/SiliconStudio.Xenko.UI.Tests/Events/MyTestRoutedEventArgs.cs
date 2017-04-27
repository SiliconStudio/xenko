// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Xenko.UI.Events;

namespace SiliconStudio.Xenko.UI.Tests.Events
{
    internal class MyTestRoutedEventArgs : RoutedEventArgs
    {
        public MyTestRoutedEventArgs(RoutedEvent routedEvent)
            : base(routedEvent)
        {
        }
    }
}
