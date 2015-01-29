using System;

namespace SiliconStudio.Core.Serialization.Assets
{
    /// <summary>
    /// A subtype of <see cref="Exception"/> thrown by the <see cref="AssetManager"/>.
    /// </summary>
    class AssetManagerException : Exception
    {
        public AssetManagerException(string message) : base(message)
        {
        }
    }
}