// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// Describes a shader parameter for a resource type.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("[{Stage}] {Param.Class} {Param.Key} -> {Param.RawName}")]
    public struct EffectParameterResourceData
    {
        /// <summary>
        /// The common description of this parameter.
        /// </summary>
        public EffectParameterData Param;

        /// <summary>
        /// The stage this parameter is used
        /// </summary>
        public ShaderStage Stage;

        /// <summary>
        /// The starting slot this parameter is bound.
        /// </summary>
        public int SlotStart;

        /// <summary>
        /// The number of slots bound to this parameter starting at <see cref="SlotStart"/>.
        /// </summary>
        public int SlotCount;
    }
}