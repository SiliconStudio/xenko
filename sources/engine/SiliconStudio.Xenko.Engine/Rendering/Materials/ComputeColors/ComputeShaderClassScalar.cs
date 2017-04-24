// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// A shader outputing a color/vector value.
    /// </summary>
    [DataContract("ComputeShaderClassScalar")]
    [Display("Shader")]
    // TODO: This class has been made abstract to be removed from the editor - unabstract it to re-enable it!
    public class ComputeShaderClassScalar : ComputeShaderClassBase<IComputeScalar>, IComputeScalar
    {
    }
}
