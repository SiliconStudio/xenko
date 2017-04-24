// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Input;

namespace SiliconStudio.Xenko.Games.Testing.Requests
{
    [DataContract]
    internal class KeySimulationRequest : TestRequestBase
    {
        public Keys Key;
        public bool Down;
    }
}
