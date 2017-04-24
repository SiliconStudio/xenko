// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering.Compositing
{
    /// <summary>
    /// A collection of <see cref="ISceneRenderer"/>.
    /// </summary>
    public partial class SceneRendererCollection : SceneRendererBase, IEnumerable<ISceneRenderer>
    {
        public List<ISceneRenderer> Children { get; } = new List<ISceneRenderer>();

        protected override void CollectCore(RenderContext context)
        {
            base.CollectCore(context);

            foreach (var child in Children)
                child.Collect(context);
        }

        protected override void DrawCore(RenderContext context, RenderDrawContext drawContext)
        {
            foreach (var child in Children)
                child.Draw(drawContext);
        }

        public void Add(ISceneRenderer child)
        {
            Children.Add(child);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<ISceneRenderer> GetEnumerator()
        {
            return Children.GetEnumerator();
        }
    }
}
