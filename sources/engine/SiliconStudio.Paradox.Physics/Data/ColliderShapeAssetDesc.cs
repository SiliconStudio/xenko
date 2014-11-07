using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<ColliderShapeAssetDesc>))]
    [DataContract("ColliderShapeAssetDesc")]
    public class ColliderShapeAssetDesc : IColliderShapeDesc
    {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
        [Browsable(false)]
#endif
        [DataMember(10)]
        public string AssetName;

        [DataMember(20)]
        public Core.Serialization.ContentReference<PhysicsColliderShapeData> Asset;
    }
}