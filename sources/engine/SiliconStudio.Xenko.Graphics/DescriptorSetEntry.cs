namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Used internally to store descriptor entries.
    /// </summary>
    internal struct DescriptorSetEntry
    {
        public object Value;

        // Used only for cbuffer
        public int Offset;
        public int Size;

        public DescriptorSetEntry(object value, int offset, int size)
        {
            Value = value;
            Offset = offset;
            Size = size;
        }
    }
}