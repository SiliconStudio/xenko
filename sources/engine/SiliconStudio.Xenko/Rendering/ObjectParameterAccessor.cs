namespace SiliconStudio.Xenko.Rendering
{
    public struct ObjectParameterAccessor<T>
    {
        internal readonly int Index;

        internal ObjectParameterAccessor(int index)
        {
            this.Index = index;
        }
    }
}