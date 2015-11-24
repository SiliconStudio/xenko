using SiliconStudio.Core;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    internal class KeySimulationRequest
    {
        public Keys Key;
        public bool Down;
    }
}