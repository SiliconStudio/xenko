// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
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
                return (Sampler.GetHashCode()*397) ^ Texture.GetHashCode();
            }
        }
    }
}