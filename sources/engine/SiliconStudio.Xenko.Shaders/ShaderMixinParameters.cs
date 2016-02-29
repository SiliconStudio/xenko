// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// Parameters used for mixin.
    /// </summary>
    [DataContract]
    public class ShaderMixinParameters : NextGenParameterCollection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderMixinParameters"/> class.
        /// </summary>
        public ShaderMixinParameters()
        {
        }
    }
}