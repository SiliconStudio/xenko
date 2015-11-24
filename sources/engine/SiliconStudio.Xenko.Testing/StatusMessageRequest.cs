using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    internal class StatusMessageRequest
    {
        public bool Error;
        public string Message;
    }
}