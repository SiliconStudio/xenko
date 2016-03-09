namespace SiliconStudio.Xenko.Rendering
{
    public struct PermutationParameter<T>
    {
        internal readonly int BindingSlot;
        internal readonly int Count;

        internal PermutationParameter(int bindingSlot, int count)
        {
            this.BindingSlot = bindingSlot;
            this.Count = count;
        }
    }
}