// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// Contains a compiled shader with bytecode for each stage.
    /// </summary>
    [DataContract]
    [ContentSerializer(typeof(DataContentSerializer<EffectBytecode>))]
    public sealed class EffectBytecode
    {
        /// <summary>
        /// The time this bytecode was compiled.
        /// </summary>
        public DateTime Time;

        /// <summary>
        /// The reflection from the bytecode.
        /// </summary>
        public EffectReflection Reflection;

        /// <summary>
        /// The used sources
        /// </summary>
        public HashSourceCollection HashSources;

        /// <summary>
        /// The bytecode for each stage.
        /// </summary>
        public ShaderBytecode[] Stages;

        /// <summary>
        /// Clones the bytecode.
        /// </summary>
        /// <returns>The cloned bytecode.</returns>
        public EffectBytecode Clone()
        {
            return (EffectBytecode)MemberwiseClone();
        }
    }
}