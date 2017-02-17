// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
