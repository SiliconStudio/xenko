namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Handle used to query what's the actual offset of a given variable in a constant buffer, through <see cref="ResourceGroupLayout.GetConstantBufferOffset"/>.
    /// </summary>
    public struct EffectDescriptorSetReference
    {
        public readonly int Index;

        internal EffectDescriptorSetReference(int index)
        {
            Index = index;
        }
    }
}