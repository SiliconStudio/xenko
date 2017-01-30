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

        protected override void CollectCore(RenderContext renderContext)
        {
            base.CollectCore(renderContext);

            foreach (var child in Children)
                child.Collect(renderContext);
        }

        protected override void DrawCore(RenderDrawContext renderContext)
        {
            foreach (var child in Children)
                child.Draw(renderContext);
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