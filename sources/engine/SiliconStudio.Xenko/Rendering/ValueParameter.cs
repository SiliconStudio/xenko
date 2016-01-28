namespace SiliconStudio.Xenko.Rendering
{
    public struct ValueParameter<T> where T : struct
    {
        internal readonly int Index;

        internal ValueParameter(int index)
        {
            this.Index = index;
        }
    }
}