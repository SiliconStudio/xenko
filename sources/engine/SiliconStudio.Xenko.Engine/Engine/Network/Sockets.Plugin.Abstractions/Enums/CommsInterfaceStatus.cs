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