namespace SiliconStudio.Xenko.Rendering
{
    public struct ValueParameter<T> where T : struct
    {
        internal readonly int Offset;
        internal readonly int Count;

        internal ValueParameter(int offset, int count)
        {
            this.Offset = offset;
            this.Count = count;
        }
    }
}