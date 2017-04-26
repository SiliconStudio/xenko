// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Games.Testing.Requests
{
    [DataContract]
    internal class TapSimulationRequest : TestRequestBase
    {
        public PointerEventType EventType;
        public TimeSpan Delta;
        public Vector2 Coords;
        public Vector2 CoordsDelta;
    }
}
