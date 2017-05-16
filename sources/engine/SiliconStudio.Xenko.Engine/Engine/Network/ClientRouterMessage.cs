// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Engine.Network
{
    /// <summary>
    /// Message exchanged between client and router.
    /// Note: shouldn't collide with <see cref="RouterMessage"/>.
    /// </summary>
    public enum ClientRouterMessage : ushort
    {
        RequestServer = 0x0000, // ClientRequestServer <string:url>
        ServerStarted = 0x0001, // ClientServerStarted <int:errorcode> <string:message>
    }
}
