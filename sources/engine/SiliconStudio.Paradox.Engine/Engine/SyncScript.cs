namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A script whose <see cref="Update"/> will be called every frame.
    /// </summary>
    public abstract class SyncScript : Script
    {
        /// <summary>
        /// Called every frame.
        /// </summary>
        public abstract void Update();
    }
}