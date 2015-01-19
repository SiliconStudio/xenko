// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Materials.ComputeColors
{
    /// <summary>
    /// A shader outputing a single scalar value.
    /// </summary>
    [DataContract("MaterialShaderClassComputeColor")]
    [Display("Shader")]
    public class MaterialShaderClassComputeColor : MaterialShaderClassComputeNodeBase<IMaterialComputeColor>, IMaterialComputeColor 
    {
    }
}
