// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.ObjectModel;
using GraphX.Controls;
using GraphX.PCL.Common.Models;

namespace SiliconStudio.Presentation.Graph.ViewModel
{
    /// <summary>
    /// Base vertex used for node-based graphs. 
    /// This class must derived from VertexBase in GraphX.
    /// </summary>
    public class NodeVertex : VertexBase
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public NodeVertex() { /* nothing */ }

        public virtual void AddOutgoing(NodeVertex target, object from, object to)
        {
        }

        public virtual void ConnectControl(VertexControl vertexControl)
        {
        }

        public virtual void DisconnectControl(VertexControl vertexControl)
        {
        }

        /// <summary>
        /// Collection of input slots
        /// </summary>
        public virtual ObservableCollection<object> InputSlots { get; set; }

        /// <summary>
        /// Collection of output slots
        /// </summary>
        public virtual ObservableCollection<object> OutputSlots { get; set; }
    }
}
