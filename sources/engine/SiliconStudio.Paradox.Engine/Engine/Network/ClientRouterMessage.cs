namespace SiliconStudio.Paradox.Engine.Network
{
    /// <summary>
    /// Message exchanged between client and router.
    /// Note: shouldn't collide with <see cref="RouterMessage"/>.
    /// </summary>
    public enum ClientRouterMessage
    {
        RequestServer = 0x00000000, // ClientRequestServer <string:url>
        ServerStarted = 0x00000001, // ClientServerStarted <int:errorcode> <string:message>
    }
}