// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering
{
    public static class GraphicsResizeExtensions
    {
        /// <summary>
        /// Sets a resizable resource for the specified key.
        /// </summary>
        /// <typeparam name="T">Type must be a <see cref="IReferencable"/></typeparam>
        /// <param name="parameterCollection">The parameter collection.</param>
        /// <param name="context">The context.</param>
        /// <param name="key">The key.</param>
        /// <param name="resourceValue">The resource value.</param>
        public static void SetWithResize<T>(this ParameterCollection parameterCollection, GraphicsResizeContext context, ParameterKey<T> key, T resourceValue) where T : IReferencable
        {
            context.SetWithResize(parameterCollection, key, resourceValue);
        }
    }
}