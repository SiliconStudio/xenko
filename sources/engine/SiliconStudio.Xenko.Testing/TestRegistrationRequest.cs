using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    public class TestRegistrationRequest
    {
        public string Cmd;
        public int Platform;
        public bool Tester;
    }

    [DataContract]
    public class TestEndedRequest
    {
        
    }

    [DataContract]
    public class StatusMessageRequest
    {
        public bool Error;
        public string Message;
    }

    [DataContract]
    public class ScreenshotRequest
    {
        public string Filename;
    }
}