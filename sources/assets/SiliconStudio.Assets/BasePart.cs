using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    [DataContract]
    public class BasePart
    {
        public BasePart(AssetReference basePartAsset, Guid basePartId, Guid instanceId)
        {
            if (basePartAsset == null) throw new ArgumentNullException(nameof(basePartAsset));
            if (basePartId == Guid.Empty) throw new ArgumentException(nameof(basePartAsset));
            if (instanceId == Guid.Empty) throw new ArgumentException(nameof(basePartAsset));
            BasePartAsset = basePartAsset;
            BasePartId = basePartId;
            InstanceId = instanceId;
        }

        public AssetReference BasePartAsset { get; }

        public Guid BasePartId { get; }

        public Guid InstanceId { get; }

        public IIdentifiable ResolvePart(PackageSession session)
        {
            var assetItem = session.FindAsset(BasePartAsset.Id);
            var asset = assetItem?.Asset as AssetComposite;
            return asset?.FindPart(BasePartId);
        }
    }
}