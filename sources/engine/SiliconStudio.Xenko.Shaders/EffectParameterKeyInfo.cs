// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// The header of a shader parameter.
    /// </summary>
    [DataContract]
    public struct EffectParameterKeyInfo
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
    }
}
