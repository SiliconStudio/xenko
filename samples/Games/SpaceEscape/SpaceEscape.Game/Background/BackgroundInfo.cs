// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Xenko.Engine;

namespace SpaceEscape.Background
{
    public class BackgroundInfo : ScriptComponent
    {
        public BackgroundInfo()
        {
            Holes = new List<Hole>();
        }

        public int MaxNbObstacles { get; set; }
        public List<Hole> Holes { get; private set; }
    }
}
