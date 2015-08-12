// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Rendering.Materials.ComputeColors
{
    /// <summary>
    /// Definition of a stream. e.g COLOR0, COLOR1...etc.
    /// </summary>
    public interface IVertexStreamDefinition
    {
        /// <summary>
        /// Gets the name of the semantic.
        /// </summary>
        /// <returns>A string with the name of the stream semantic.</returns>
        string GetSemanticName();
    }
}