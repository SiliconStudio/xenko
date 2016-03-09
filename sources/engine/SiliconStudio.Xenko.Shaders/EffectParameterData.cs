// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// The header of a shader parameter.
    /// </summary>
    [DataContract]
    public struct EffectParameterData
    {
        /// <summary>
        /// The key of the parameter.
        /// </summary>
        [DataMemberIgnore]
        public ParameterKey Key;

        /// <summary>
        /// The key name.
        /// </summary>
        public string KeyName;

        /// <summary>
        /// The index of the parameter.
        /// </summary>
        public int KeyIndex;

        /// <summary>
        /// Resource group this variable belongs to. This should later be directly grouped in EffectReflection.ResourceGroups.
        /// </summary>
        public string ResourceGroup;

        /// <summary>
        /// Name of this parameter in the original shader
        /// </summary>
        public string RawName;

        /// <summary>
        /// The <see cref="EffectParameterClass"/> of this parameter.
        /// </summary>
        public EffectParameterClass Class;

        /// <summary>
        /// The <see cref="EffectParameterType"/> of this parameter.
        /// </summary>
        public EffectParameterType Type;
    }
}