// Copyright(c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    // Avoid heap allocations, by pooling semantic types lazily.
    internal static class CompositionExtension
    {
        private static readonly Dictionary<Type, IRenderTargetSemantic> semanticsPool = new Dictionary<Type, IRenderTargetSemantic>();

        internal static void Add<T>(this RenderTargetSetup composition)
            where T : IRenderTargetSemantic, new()
        {
            IRenderTargetSemantic semantic;
            if (!semanticsPool.TryGetValue(typeof(T), out semantic))
            {
                semantic = new T();
            }

            composition.AddTarget(new RenderTarget
            {
                Description = new RenderTargetDesc { Semantic = semantic }
            });
        }
    }
}