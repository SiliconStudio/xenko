// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core.Diagnostics;

namespace SiliconStudio.Xenko.Engine.Network
{
    /// <summary>
    /// In the case of a SocketMessage when we use it in a SendReceiveAsync we want to propagate exceptions from the remote host
    /// </summary>
    public class ExceptionMessage : SocketMessage
    {
        /// <summary>
        /// Remote exception information
        /// </summary>
        public ExceptionInfo ExceptionInfo;
    }
}
