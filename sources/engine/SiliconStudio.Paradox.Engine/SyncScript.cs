using SiliconStudio.Core;

namespace SiliconStudio.Paradox
{
    /// <summary>
    /// A script whose <see cref="Update"/> will be called every frame.
    /// </summary>
    [DataContract("SyncScript")]
    public abstract class SyncScript : Script
    {
        /// <summary>
        /// Called every frame.
        /// </summary>
        public abstract void Update();
    }
}