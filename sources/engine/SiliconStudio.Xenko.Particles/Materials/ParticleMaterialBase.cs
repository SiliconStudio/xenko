// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Particles.Materials
{
    public enum ParticleMaterialCulling : byte
    {
        CullNone = 0,
        CullBack = 1,
        CullFront = 2
    }

    [DataContract("ParticleMaterialBase")]
    public abstract class ParticleMaterialBase
    {
        [DataMember(20)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Alpha-Additive lerp")]
        public float AlphaAdditive { get; set; }


        [DataMember(40)]
        [Display("Face culling")]
        public ParticleMaterialCulling FaceCulling;

    }
}
