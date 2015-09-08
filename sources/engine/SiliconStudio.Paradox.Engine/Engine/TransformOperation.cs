namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Performs some work after world matrix has been updated.
    /// </summary>
    public abstract class TransformOperation
    {
        public abstract void Process(TransformComponent transformComponent);
    }
}