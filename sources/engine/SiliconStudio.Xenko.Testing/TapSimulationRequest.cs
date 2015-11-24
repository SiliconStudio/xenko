using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Testing
{
    [DataContract]
    internal class TapSimulationRequest
    {
        public PointerState State;
        public TimeSpan Delta;
        public Vector2 Coords;
        public Vector2 CoordsDelta;
    }
}