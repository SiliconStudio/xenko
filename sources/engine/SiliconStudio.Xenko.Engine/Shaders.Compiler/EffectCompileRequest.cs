// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    /// <summary>
    /// Represents an effect compile request done to the <see cref="EffectSystem"/>.
    /// </summary>
    [DataContract("EffectCompileRequest")]
    [DataSerializerGlobal(null, typeof(KeyValuePair<EffectCompileRequest, bool>))]
    public class EffectCompileRequest : IEquatable<EffectCompileRequest>
    {
        public string EffectName;
        public CompilerParameters UsedParameters;

        public EffectCompileRequest()
        {
        }

        public EffectCompileRequest(string effectName, CompilerParameters usedParameters)
        {
            EffectName = effectName;
            UsedParameters = usedParameters;
        }

        public bool Equals(EffectCompileRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(EffectName, other.EffectName) && ShaderMixinObjectId.Compute(EffectName, UsedParameters) == ShaderMixinObjectId.Compute(other.EffectName, other.UsedParameters);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EffectCompileRequest)obj);
        }

        public override int GetHashCode()
        {
            return ShaderMixinObjectId.Compute(EffectName, UsedParameters).GetHashCode();
        }
    }
}