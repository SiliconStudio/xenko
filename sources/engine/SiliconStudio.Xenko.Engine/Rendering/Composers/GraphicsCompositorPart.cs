using System;
using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    public interface IGraphicsCompositorPart : IIdentifiable, IRenderCollector, IGraphicsRendererBase
    {
    }

    public interface IGraphicsCompositorSharedPart : IGraphicsCompositorPart
    {
        string Name { get; }
    }

    /// <summary>
    /// Describes the code part of a <see cref="GraphicsCompositor"/>.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class GraphicsCompositorPart : IGraphicsCompositorPart
    {
        /// <inheritdoc/>
        [DataMember(-100), Display(Browsable = false)]
        public Guid Id { get; set; }

        public virtual string Name => GetType().Name;

        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;
        public bool Initialized { get; private set; }

        protected GraphicsCompositorPart()
        {
            Id = Guid.NewGuid();
        }

        public void Initialize(RenderContext context)
        {
            Initialized = true;
        }

        public void Dispose()
        {
        }

        public virtual void Collect(RenderContext renderContext)
        {
        }

        public virtual void Draw(RenderDrawContext renderContext)
        {
        }
    }
}