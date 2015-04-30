using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Shaders.Compiler
{
    /// <summary>
    /// Represents an effect compile request done to the <see cref="EffectSystem"/>.
    /// </summary>
    [DataContract("EffectCompileRequest")]
    [DataSerializerGlobal(null, typeof(KeyValuePair<EffectCompileRequest, bool>))]
    public class EffectCompileRequest : IEquatable<EffectCompileRequest>
    {
        public string EffectName;
        public ShaderMixinParameters UsedParameters;

        public EffectCompileRequest()
        {
        }

        public EffectCompileRequest(string effectName, ShaderMixinParameters usedParameters)
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
            unchecked
            {
                return ShaderMixinObjectId.Compute(EffectName, UsedParameters).GetHashCode();
            }
        }
    }
}