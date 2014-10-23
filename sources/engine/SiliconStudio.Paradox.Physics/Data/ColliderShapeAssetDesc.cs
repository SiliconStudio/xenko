using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Contents;

namespace SiliconStudio.Paradox.Physics
{
    [ContentSerializer(typeof(DataContentSerializer<ColliderShapeAssetDesc>))]
    [DataContract("ColliderShapeAssetDesc")]
    public class ColliderShapeAssetDesc : IColliderShapeDesc
    {
        [DataMember(10)]
        [Browsable(false)]
        public string AssetName;

        [DataMember(20)]
        public Core.Serialization.ContentReference<PhysicsColliderShapeData> Asset;
    }
}