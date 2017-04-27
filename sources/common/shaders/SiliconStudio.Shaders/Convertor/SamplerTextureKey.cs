// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Shaders.Ast;

namespace SiliconStudio.Shaders.Convertor
{
    struct SamplerTextureKey : IEquatable<SamplerTextureKey>
    {
        public Variable Sampler;
        public Variable Texture;

        public SamplerTextureKey(Variable sampler, Variable texture)
        {
            Sampler = sampler;
            Texture = texture;
        }

        public bool Equals(SamplerTextureKey other)
        {
            if (Sampler == null && other.Sampler == null)
                return Texture.Equals(other.Texture);
            if (Sampler == null || other.Sampler == null)
                return false;
            return Sampler.Equals(other.Sampler) && Texture.Equals(other.Texture);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SamplerTextureKey && Equals((SamplerTextureKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashcode = Sampler == null ? 1 : Sampler.GetHashCode();
                return (hashcode*397) ^ Texture.GetHashCode();
            }
        }
    }
}
