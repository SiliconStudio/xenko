// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles
{
    [Flags]
    public enum InheritLocation
    {
        Position = 1,
        Rotation = 2,
        Scale = 4,
    }

    [DataContract("PaticleModuleBase")]
    public abstract class ParticleModuleBase
    {
        internal List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);

        public abstract void SetParentTRS(ref Vector3 Translation, ref Quaternion Rotation, float Scale);

    }
}
