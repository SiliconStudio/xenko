namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Handle used to query logical group information.
    /// </summary>
    public struct LogicalGroupReference
    {
        public static readonly LogicalGroupReference Invalid = new LogicalGroupReference(-1);

        internal int Index;

        internal LogicalGroupReference(int index)
        {
            Index = index;
        }
    }
}