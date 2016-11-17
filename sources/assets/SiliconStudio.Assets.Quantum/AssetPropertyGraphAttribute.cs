using System;

namespace SiliconStudio.Assets.Quantum
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class AssetPropertyGraphAttribute : Attribute
    {
        public AssetPropertyGraphAttribute(Type assetType)
        {
            if (assetType == null) throw new ArgumentNullException(nameof(assetType));
            if (!typeof(Asset).IsAssignableFrom(assetType)) throw new ArgumentException($"The given type must be assignable to the {nameof(Asset)} type.");
            AssetType = assetType;
        }

        public Type AssetType { get; }
    }
}
