using System;
using System.Collections;
using System.Collections.Generic;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A collection of <see cref="RenderObject"/>.
    /// </summary>
    public class RenderObjectCollection : ICollection<RenderObject>
    {
        private readonly NextGenRenderSystem renderSystem;
        private readonly List<RenderObject> items = new List<RenderObject>();

        internal RenderObjectCollection(NextGenRenderSystem renderSystem)
        {
            this.renderSystem = renderSystem;
        }

        public void Add(RenderObject renderObject)
        {
            renderSystem.AddRenderObject(items, renderObject);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(RenderObject renderObject)
        {
            return items.Contains(renderObject);
        }

        public void CopyTo(RenderObject[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(RenderObject renderObject)
        {
            return renderSystem.RemoveRenderObject(items, renderObject);
        }

        public int Count => items.Count;

        public bool IsReadOnly => false;

        public List<RenderObject>.Enumerator GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator<RenderObject> IEnumerable<RenderObject>.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }
    }
}