// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Xenko.UI.Events
{
    /// <summary>
    /// Indicates the routing strategy of a routed event.
    /// </summary>
    public enum RoutingStrategy
    {
        /// <summary>
        /// The routed event uses a bubbling strategy, where the event instance routes upwards through the tree, from event source to root.
        /// </summary>
        /// <userdoc>The routed event uses a bubbling strategy, where the event instance routes upwards through the tree, from event source to root.</userdoc>
        Bubble,
        /// <summary>
        /// The routed event uses a tunneling strategy, where the event instance routes downwards through the tree, from root to source element.
        /// </summary>
        /// <userdoc>The routed event uses a tunneling strategy, where the event instance routes downwards through the tree, from root to source element.</userdoc>
        Tunnel,
        /// <summary>
        /// The routed event does not route through an element tree, but does support other routed event capabilities such as class handling.
        /// </summary>
        /// <userdoc>The routed event does not route through an element tree, but does support other routed event capabilities such as class handling.</userdoc>
        Direct,
    }
}