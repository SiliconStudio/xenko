// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Describes the state of a <see cref="RenderEffect"/>.
    /// </summary>
    public enum RenderEffectState
    {
        /// <summary>
        /// The effect is in normal state.
        /// </summary>
        Normal = 0,

        /// <summary>
        /// The effect is being asynchrounously compiled.
        /// </summary>
        Compiling = 1,
        
        /// <summary>
        /// There was an error while compiling the effect.
        /// </summary>
        Error = 2,

        /// <summary>
        /// The effect is skipped.
        /// </summary>
        Skip = 3,
    }
}
