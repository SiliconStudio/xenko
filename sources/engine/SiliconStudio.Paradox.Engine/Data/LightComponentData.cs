// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

namespace SiliconStudio.Paradox.Engine.Data
{
    public partial class LightComponentData
    {
        public LightComponentData()
        {
            Enabled = true;
            // Default light color is white (better than black)
            Color = new Core.Mathematics.Color3(1.0f);
        }
    }
}