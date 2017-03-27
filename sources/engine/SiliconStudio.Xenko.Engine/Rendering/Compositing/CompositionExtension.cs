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

        // Recycle an instance when needed. It is possible as long as implementations respect the stateless contract.
        internal static IRenderTargetSemantic ScoopSemantic(Type semanticType)
        {
            if (semanticsPool.ContainsKey(semanticType))
                return semanticsPool[semanticType];

            IRenderTargetSemantic semantic = (IRenderTargetSemantic)Activator.CreateInstance(semanticType);
            semanticsPool[semanticType] = semantic;

            return semantic;
        }

        internal static void AddTargetTo<TSemantic>(this RenderTargetSetup composition)
        {
            composition.AddTarget(new RenderTarget()
            {
                Description = new RenderTargetDesc() { Semantic = ScoopSemantic(typeof(TSemantic)) }
            });
        }
    }
}