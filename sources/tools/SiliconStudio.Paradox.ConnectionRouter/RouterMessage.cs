using SiliconStudio.Paradox.Engine.Network;

namespace SiliconStudio.Paradox.ConnectionRouter
{
    public enum RouterMessage
    {
        ClientRequestServer = ClientRouterMessage.RequestServer, // ClientRequestServer <string:url>
        ClientServerStarted = ClientRouterMessage.ServerStarted, // ClientServerStarted <varint:errorcode> <string:message optional(if errorcode != 0)>

        ServerStarted = 0x00000100, // ServerStarted <guid:token> <varint:errorcode> <string:message optional(if errorcode != 0)>

        ServiceProvideServer = 0x00001000, // ProvideServer <string:url>
        ServiceRequestServer = 0x00001001, // RequestServer <string:url> <guid:token>
    }
}