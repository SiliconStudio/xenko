using System;
using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering.Composers
{
    public interface IGraphicsCompositorPart : IRenderCollector, IGraphicsRendererBase
    {
    }

    public interface IGraphicsCompositorSharedPart : IIdentifiable
    {
        string Name { get; }
    }

    /// <summary>
    /// Describes the code part of a <see cref="GraphicsCompositor"/>.
    /// </summary>
    [DataContract(Inherited = true)]
    public abstract class GraphicsCompositorPart : RendererBase, IGraphicsCompositorPart
    {
        /// <inheritdoc/>
        [DataMember(-100), Display(Browsable = false)]
        public Guid Id { get; set; }

        public override string Name => GetType().Name;

        protected GraphicsCompositorPart()
        {
            Id = Guid.NewGuid();
        }

        /// <inheritdoc/>
        public virtual void Collect(RenderContext renderContext)
        {
        }
    }
}