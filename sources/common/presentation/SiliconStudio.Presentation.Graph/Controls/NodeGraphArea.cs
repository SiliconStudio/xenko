// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using QuickGraph;
using SiliconStudio.Presentation.Graph.ViewModel;
using System.Windows;
using GraphX.Controls;

namespace SiliconStudio.Presentation.Graph.Controls
{    
    /// <summary>
    /// 
    /// </summary>
    public class NodeGraphArea : GraphArea<NodeVertex, NodeEdge, BidirectionalGraph<NodeVertex, NodeEdge>> 
    {
        public virtual event LinkSelectedEventHandler LinkSelected;

        internal virtual void OnLinkSelected(FrameworkElement link)
        {
            LinkSelected?.Invoke(this, new LinkSelectedEventArgs(link));
        }
    }
}
