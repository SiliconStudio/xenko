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