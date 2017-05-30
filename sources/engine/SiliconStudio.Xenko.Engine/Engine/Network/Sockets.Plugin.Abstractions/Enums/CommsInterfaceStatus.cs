// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace Sockets.Plugin.Abstractions
{
    /// <summary>
    /// The connection state of an interface.
    /// </summary>
    enum CommsInterfaceStatus
    {
        /// <summary>
        /// The state of the interface can not be determined.
        /// </summary>
        Unknown,

        /// <summary>
        /// The interface is connected. 
        /// </summary>
        Connected,

        /// <summary>
        /// The interface is disconnected.
        /// </summary>
        Disconnected,
    }
}
