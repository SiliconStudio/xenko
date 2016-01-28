namespace SiliconStudio.Xenko.Rendering
{
    public struct ResourceParameter<T> where T : class
    {
        internal readonly int Index;

        internal ResourceParameter(int index)
        {
            this.Index = index;
        }
    }
}