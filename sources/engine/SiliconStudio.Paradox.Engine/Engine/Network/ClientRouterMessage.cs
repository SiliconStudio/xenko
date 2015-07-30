// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Engine.Network
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