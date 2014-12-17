// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.DataModel
{
    [DataContract("LightType")]
    public enum LightType
    {
        Point,
        Spherical,
        Directional,
        Spot,
    }
}