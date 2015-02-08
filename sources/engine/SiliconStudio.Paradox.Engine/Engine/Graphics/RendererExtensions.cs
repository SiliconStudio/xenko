// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Extensions for <see cref="Renderer"/>
    /// </summary>
    public static class RendererExtensions
    {
        /// <summary>
        /// Appends a name to an existing <see cref="Renderer.DebugName"/>.
        /// </summary>
        /// <typeparam name="T">Type of the renderer</typeparam>
        /// <param name="renderer">The renderer</param>
        /// <param name="name">Name to append to <see cref="Renderer.DebugName"/>.</param>
        /// <returns>The renderer</returns>
        public static T AppendDebugName<T>(this T renderer, string name) where T : Renderer
        {
            renderer.DebugName = renderer.DebugName == null ? name : renderer.DebugName + " " + name;
            return renderer;
        }
    }
}