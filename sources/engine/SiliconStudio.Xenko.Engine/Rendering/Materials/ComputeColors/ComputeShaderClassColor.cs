// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A shader outputing a single scalar value.
    /// </summary>
    [DataContract("ComputeShaderClassColor")]
    [Display("Shader")]
    // TODO: This class has been made abstract to be removed from the editor - unabstract it to re-enable it!
    public class ComputeShaderClassColor : ComputeShaderClassBase<IComputeColor>, IComputeColor 
    {
        /// <inheritdoc/>
        public bool HasChanged
        {
            get
            {
                // Shaders are too generic so we always return true.
                return true;
            }
        }
    }
}
