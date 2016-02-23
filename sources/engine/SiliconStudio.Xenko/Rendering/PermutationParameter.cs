namespace SiliconStudio.Xenko.Rendering
{
    public struct PermutationParameter<T>
    {
        internal readonly int Index;

        internal PermutationParameter(int index)
        {
            this.Index = index;
        }
    }
}