using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace SiliconStudio.Presentation.Drawing
{
    /// <summary>
    /// Provides a hosting <see cref="FrameworkElement"/> for a collection of <see cref="Visual"/>.
    /// </summary>
    internal class VisualHost : FrameworkElement
    {
        private readonly VisualCollection children;

        public VisualHost()
        {
            children = new VisualCollection(this);
        }
        
        /// <inheritdoc/>
        protected override int VisualChildrenCount => children.Count;

        public int AddChild(Visual child)
        {
            return children.Add(child);
        }

        public void AddChildren(IEnumerable<Visual> visuals)
        {
            foreach (var child in children)
            {
                children.Add(child);
            }
        }

        /// <inheritdoc/>
        protected override Visual GetVisualChild(int index)
        {
            return children[index];
        }
    }
}
