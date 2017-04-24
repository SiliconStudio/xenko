// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Diagnostics;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Shaders
{
    /// <summary>
    /// Describes a shader parameter member.
    /// </summary>
    [DataContract]
    [DebuggerDisplay("{Name}: {Type}")]
    public struct EffectTypeMemberDescription
    {
        /// <summary>
        /// The name of this member.
        /// </summary>
        public string Name;

        /// <summary>
        /// Offset in bytes into the parent structure (0 if not a structure member).
        /// </summary>
        public int Offset;

        /// <summary>
        /// The type of this member.
        /// </summary>
        public EffectTypeDescription Type;
    }
}
