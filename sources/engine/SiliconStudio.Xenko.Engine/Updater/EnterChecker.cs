namespace SiliconStudio.Xenko.Updater
{
    /// <summary>
    /// Provides a way to perform additional checks when entering an object (typically out of bounds checks).
    /// </summary>
    public abstract class EnterChecker
    {
        /// <summary>
        /// Called by <see cref="UpdateEngine.Run"/> to perform additional checks when entering an object (typically out of bounds checks).
        /// </summary>
        /// <param name="obj">The object being entered.</param>
        /// <returns>True if checks succeed, false otherwise.</returns>
        public abstract bool CanEnter(object obj);
    }
}