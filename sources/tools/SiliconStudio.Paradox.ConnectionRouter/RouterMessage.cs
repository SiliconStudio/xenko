using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.ConnectionRouter
{
    public enum RouterMessage : ushort
    {
        ClientRequestServer = ClientRouterMessage.RequestServer, // ClientRequestServer <string:url>
        ClientServerStarted = ClientRouterMessage.ServerStarted, // ClientServerStarted <int:errorcode> <string:message optional(if errorcode != 0)>

        ServiceProvideServer = 0x1000, // ProvideServer <string:url>
        ServiceRequestServer = 0x1001, // RequestServer <string:url> <guid:token>

        ServerStarted = 0x2000, // ServerStarted <guid:token> <varint:errorcode> <string:message optional(if errorcode != 0)>
    }
}