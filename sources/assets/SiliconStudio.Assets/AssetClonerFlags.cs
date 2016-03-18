// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Flags used by <see cref="AssetCloner.Clone"/>
    /// </summary>
    [Flags]
    public enum AssetClonerFlags
    {
        /// <summary>
        /// No special flags while cloning.
        /// </summary>
        None,

        /// <summary>
        /// Remove all attached overrides information when cloning (equivalent to revert everything to <see cref="SiliconStudio.Core.Reflection.OverrideType.Base"/>)
        /// </summary>
        RemoveOverrides = 1,

        /// <summary>
        /// Attached references will be cloned as <c>null</c>
        /// </summary>
        ReferenceAsNull = 2,

        /// <summary>
        /// Keep cloned bases.
        /// </summary>
        KeepBases = 4,
    }
}