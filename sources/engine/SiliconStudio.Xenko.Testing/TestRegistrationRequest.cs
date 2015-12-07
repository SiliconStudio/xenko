using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    internal class TestRegistrationRequest
    {
        public string Cmd;
        public string GameAssembly;
        public int Platform;
        public bool Tester;
    }
}