// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.SpriteStudio.Offline
{
    [DataContract]
    public class NodeAnimationData
    {
        public NodeAnimationData()
        {
            Data = new Dictionary<string, List<Dictionary<string, string>>>();
        }

        public Dictionary<string, List<Dictionary<string, string>>> Data;
    }
}
