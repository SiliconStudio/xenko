// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// Binding to a sampler.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("SamplerState {Key} ({Description.Filter})")]
    public class EffectSamplerStateBinding
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EffectSamplerStateBinding"/> class.
        /// </summary>
        public EffectSamplerStateBinding()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EffectSamplerStateBinding"/> class.
        /// </summary>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="description">The description.</param>
        public EffectSamplerStateBinding(string keyName, SamplerStateDescription description)
        {
            KeyName = keyName;
            Description = description;
        }

        /// <summary>
        /// The key used to bind this sampler, used internaly.
        /// </summary>
        [DataMemberIgnore]
        public ParameterKey Key;

        /// <summary>
        /// The key name.
        /// </summary>
        public string KeyName;

        /// <summary>
        /// The description of this sampler.
        /// </summary>
        public SamplerStateDescription Description;
    }
}
