// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Xenko.Engine.Network;

namespace SiliconStudio.Xenko.ConnectionRouter
{
    public enum RouterMessage : ushort
    {
        ClientRequestServer = ClientRouterMessage.RequestServer, // ClientRequestServer <string:url>
        ClientServerStarted = ClientRouterMessage.ServerStarted, // ClientServerStarted <int:errorcode> <string:message optional(if errorcode != 0)>

        ServiceProvideServer = 0x1000, // ProvideServer <string:url>       
        ServiceRequestServer = 0x1001, // RequestServer <string:url> <guid:token>
        TaskProvideServer = 0x1002, // ProvideServer <string:url>

        ServerStarted = 0x2000, // ServerStarted <guid:token> <varint:errorcode> <string:message optional(if errorcode != 0)>
    }
}
