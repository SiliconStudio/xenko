// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Xenko.Rendering
{
    public enum FallbackEffectType
    {
        /// <summary>
        /// The effect is being asynchrounously compiled.
        /// </summary>
        Compiling = 1,
        
        /// <summary>
        /// There was an error while compiling the effect.
        /// </summary>
        Error = 2,
    }
}