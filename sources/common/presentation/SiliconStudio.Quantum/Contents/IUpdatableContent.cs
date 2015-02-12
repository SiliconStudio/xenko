namespace SiliconStudio.Quantum.Contents
{
    /// <summary>
    /// Represents an <see cref="IContent"/> type that must refresh node references when its value is modified.
    /// </summary>
    public interface IUpdatableContent
    {
        void RegisterOwner(IModelNode node);
    }
}