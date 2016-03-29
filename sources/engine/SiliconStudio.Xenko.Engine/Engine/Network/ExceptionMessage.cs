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
