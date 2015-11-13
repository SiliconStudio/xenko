using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    public class StatusMessageRequest
    {
        public bool Error;
        public string Message;
    }
}