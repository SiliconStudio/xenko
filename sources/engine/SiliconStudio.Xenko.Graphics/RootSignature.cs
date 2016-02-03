namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Describes how <see cref="DescriptorSet"/> will be bound together.
    /// </summary>
    public class RootSignature : GraphicsResourceBase
    {
        DescriptorSetLayout[] layouts;

        public static RootSignature New(GraphicsDevice graphicsDevice, DescriptorSetLayout[] layouts)
        {
            return new RootSignature(graphicsDevice, layouts);
        }

        private RootSignature(GraphicsDevice graphicsDevice, DescriptorSetLayout[] layouts)
            : base(graphicsDevice)
        {
            this.layouts = layouts;
        }
    }
}