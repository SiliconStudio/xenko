using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Rendering
{
    public interface IGraphicsCompositorPart : IIdentifiable
    {
    }

    public interface IGraphicsCompositorSharedPart : IGraphicsCompositorPart
    {
        string Name { get; }
    }

    public interface IGraphicsCompositorTopPart : IGraphicsCompositorPart
    {
        void Collect(RenderContext renderContext);

        void Draw(RenderDrawContext renderContext);
    }

    public interface IGraphicsCompositorViewPart : IGraphicsCompositorPart
    {
        void Collect(RenderContext renderContext, RenderView mainRenderView);

        void Draw(RenderDrawContext renderContext, RenderView mainRenderView);
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

        protected GraphicsCompositorPart()
        {
            Id = Guid.NewGuid();
        }
    }

    public abstract class GraphicsCompositorTopPart : GraphicsCompositorPart, IGraphicsCompositorTopPart
    {
        public virtual void Collect(RenderContext renderContext)
        {
        }

        public virtual void Draw(RenderDrawContext renderContext)
        {
        }
    }

    public abstract class GraphicsCompositorViewPart : GraphicsCompositorPart, IGraphicsCompositorViewPart
    {
        public virtual void Collect(RenderContext renderContext, RenderView mainRenderView)
        {
        }

        public virtual void Draw(RenderDrawContext renderContext, RenderView mainRenderView)
        {
        }
    }
}