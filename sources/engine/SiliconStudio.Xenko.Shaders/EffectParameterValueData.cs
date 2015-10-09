// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;

namespace SiliconStudio.Paradox.Shaders
{
    /// <summary>
    /// Describes a shader parameter for a valuetype (usually stored in constant buffers).
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Param.Class}{RowCount}x{ColumnCount} {Param.KeyName} -> {Param.RawName}")]
    public struct EffectParameterValueData
    {
        /// <summary>
        /// The common description of this parameter.
        /// </summary>
        public EffectParameterData Param;

        /// <summary>
        /// Source Offset in bytes from the parameter.
        /// </summary>
        public int SourceOffset;

        /// <summary>
        /// Offset in bytes into the constant buffer.
        /// </summary>
        public int Offset;

        /// <summary>
        /// Number of elements.
        /// </summary>
        public int Count;

        /// <summary>
        /// Size in bytes in a constant buffer.
        /// </summary>
        public int Size;

        /// <summary>
        /// Number of rows for this element.
        /// </summary>
        public int RowCount;

        /// <summary>
        /// Number of columns for this element.
        /// </summary>
        public int ColumnCount;

        /// <summary>
        /// The default value.
        /// </summary>
        public byte[] DefaultValue;
    }
}