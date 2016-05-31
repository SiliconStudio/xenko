namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Handle used to query what's the actual offset of a given variable in a constant buffer, through <see cref="ResourceGroupLayout.GetConstantBufferOffset"/>.
    /// </summary>
    public struct ConstantBufferOffsetReference
    {
        public static readonly ConstantBufferOffsetReference Invalid = new ConstantBufferOffsetReference(-1);

        internal int Index;

        internal ConstantBufferOffsetReference(int index)
        {
            Index = index;
        }
    }
}