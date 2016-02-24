using System;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Rendering
{
    public abstract class VisibilityObject
    {
        public RenderObject RenderObject;
    }

    /// <summary>
    /// Describes something that can be rendered by a <see cref="RootRenderFeature"/>.
    /// </summary>
    public abstract class RenderObject
    {
        private EntityGroup renderGroup;

        /// <summary>
        /// Defines which render groups this object belongs to. Note that this is evaluated only at insertion time.
        /// Dynamic changes are not implemented yet.
        /// </summary>
        public EntityGroup RenderGroup
        {
            get { return renderGroup; }
            set
            {
                // TODO GRAPHICS REFACTOR implement dynamic render groups changes
                if (RenderFeature != null)
                    throw new NotImplementedException();

                renderGroup = value;
            }
        }

        public bool Enabled = true;

        public BoundingBoxExt BoundingBox;

        // Kept in cache to quickly know if RenderPerFrameNode was already generated
        public RootRenderFeature RenderFeature;
        public ObjectNodeReference ObjectNode;

        public StaticObjectNodeReference StaticCommonObjectNode = StaticObjectNodeReference.Invalid;
        public StaticObjectNodeReference StaticObjectNode = StaticObjectNodeReference.Invalid;

        public ActiveRenderStage[] ActiveRenderStages;

        // TODO: Switch to a "StaticPropertyContainer" that will be optimized by assembly processor
        //public PropertyContainer Tags;
    }
}