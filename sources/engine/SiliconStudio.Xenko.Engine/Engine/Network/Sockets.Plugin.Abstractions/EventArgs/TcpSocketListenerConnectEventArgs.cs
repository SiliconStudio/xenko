// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;

namespace Sockets.Plugin.Abstractions
{
    /// <summary>
    ///     Fired when a TcpSocketListener receives a new connection.
    /// </summary>
    class TcpSocketListenerConnectEventArgs : EventArgs
    {
        private readonly ITcpSocketClient _socketClient;

        /// <summary>
        ///     A <code>TcpSocketClient</code> representing the newly connected client.
        /// </summary>
        public ITcpSocketClient SocketClient
        {
            get { return _socketClient; }
        }

        /// <summary>
        ///     Constructor for <code>TcpSocketListenerConnectEventArgs.</code>
        /// </summary>
        /// <param name="socketClient">A <code>TcpSocketClient</code> representing the newly connected client.</param>
        public TcpSocketListenerConnectEventArgs(ITcpSocketClient socketClient)
        {
            _socketClient = socketClient;
        }
    }
}
