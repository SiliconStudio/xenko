namespace RenderArchitecture
{
    /// <summary>
    /// Represents an single render operation of a <see cref="RenderObject"/> from a specific view with a specific effect, with attached properties.
    /// </summary>
    struct RenderNode
    {
        /// <summary>
        /// Underlying render object.
        /// </summary>
        public readonly RenderObject RenderObject;

        /// <summary>
        /// View used when rendering. This is usually a frustum and some camera parameters.
        /// </summary>
        public readonly RenderView RenderView;

        /// <summary>
        /// Contains parameters specific to this object in the current view.
        /// </summary>
        public readonly ViewObjectNodeReference ViewObjectNode;

        /// <summary>
        /// Contains parameters specific to this object with the current effect.
        /// </summary>
        public EffectObjectNodeReference EffectObjectNode;

        /// <summary>
        /// The "PerDraw" constant buffer offset in our global cbuffer.
        /// </summary>
        public int DrawConstantBufferOffset;

        /// <summary>
        /// The "PerDraw" descriptor set.
        /// </summary>
        public DescriptorSet DrawDescriptorSet;

        /// <summary>
        /// The render stage.
        /// </summary>
        public RenderStage RenderStage;

        /// <summary>
        /// The render effect.
        /// </summary>
        public RenderEffect RenderEffect;

        public RenderNode(RenderObject renderObject, RenderView renderView, ViewObjectNodeReference viewObjectNode, RenderStage renderStage)
        {
            RenderObject = renderObject;
            RenderView = renderView;
            ViewObjectNode = viewObjectNode;
            EffectObjectNode = EffectObjectNodeReference.Invalid;
            RenderStage = renderStage;
            DrawConstantBufferOffset = 0;
            RenderEffect = null;
            DrawDescriptorSet = new DescriptorSet();
        }
    }
}