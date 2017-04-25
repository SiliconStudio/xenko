// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Linq;
using System.Windows;

namespace SiliconStudio.Presentation.Graph
{
    /// <summary>
    /// 
    /// </summary>
    public class LinkSelectedEventArgs : EventArgs
    {
        public FrameworkElement Link { get; private set; }

        public LinkSelectedEventArgs(FrameworkElement link)
            : base()
        {
            Link = link;
        }
    }

    public delegate void LinkSelectedEventHandler(object sender, LinkSelectedEventArgs args);
}
