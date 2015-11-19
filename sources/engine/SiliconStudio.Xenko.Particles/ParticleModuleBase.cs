// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("PArticleModuleBase")]
    public abstract class ParticleModuleBase
    {
        internal List<ParticleFieldDescription> RequiredFields = new List<ParticleFieldDescription>(ParticlePool.DefaultMaxFielsPerPool);
    }
}
