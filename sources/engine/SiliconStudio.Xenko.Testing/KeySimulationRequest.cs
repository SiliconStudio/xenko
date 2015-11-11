using System.Security.Policy;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    public class KeySimulationRequest
    {
        public Keys Key;
        public bool Down;
    }

    [DataContract]
    public class TapSimulationRequest
    {
        public bool Down;
        public Vector2 Coords;
    }
}