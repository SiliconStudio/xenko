using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    internal class TapSimulationRequest
    {
        public bool Down;
        public Vector2 Coords;
    }
}