// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Common interface for the Cel Light part of a cel shading model.
    /// </summary>
    public interface IMaterialCelShadingLightFunction
    {
        /// <summary>
        /// Generates the shader class source used for the shader composition.
        /// </summary>
        /// <returns>ShaderSource.</returns>
        ShaderSource Generate(MaterialGeneratorContext context);
    }
}
