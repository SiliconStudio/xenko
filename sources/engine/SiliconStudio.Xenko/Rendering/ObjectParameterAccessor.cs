namespace SiliconStudio.Xenko.Rendering
{
    public struct ObjectParameterAccessor<T>
    {
        internal readonly int BindingSlot;
        internal readonly int Count;

        internal ObjectParameterAccessor(int bindingSlot, int count)
        {
            this.BindingSlot = bindingSlot;
            this.Count = count;
        }
    }
}