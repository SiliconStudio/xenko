// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Rendering.Materials
{
    /// <summary>
    /// Common interface for a microfacet function.
    /// </summary>
    public interface IMaterialSpecularMicrofacetFunction
    {
        /// <summary>
        /// Generates the shader class source used for the shader composition.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>ShaderSource.</returns>
        ShaderSource Generate(MaterialGeneratorContext context);
    }
}
