namespace RenderArchitecture
{
    /// <summary>
    /// Represents a <see cref="RenderObject"/> and allows to attach properties every frame.
    /// </summary>
    struct ObjectNode
    {
        /// <summary>
        /// Access underlying RenderObject.
        /// </summary>
        public RenderObject RenderObject;

        public ObjectNode(RenderObject renderObject)
        {
            RenderObject = renderObject;
        }
    }
}