using SiliconStudio.Assets;
using SiliconStudio.Xenko.Physics;

namespace SiliconStudio.Xenko.Assets.Physics
{
    public class ColliderShapeBoxFactory : AssetFactory<ColliderShapeAsset>
    {
        public static ColliderShapeAsset Create()
        {
            return new ColliderShapeAsset { ColliderShapes = { new BoxColliderShapeDesc() } };
        }

        public override ColliderShapeAsset New()
        {
            return Create();
        }
    }

    public class ColliderShapeCapsuleFactory : AssetFactory<ColliderShapeAsset>
    {
        public static ColliderShapeAsset Create()
        {
            return new ColliderShapeAsset { ColliderShapes = { new CapsuleColliderShapeDesc() } };
        }

        public override ColliderShapeAsset New()
        {
            return Create();
        }
    }

    public class ColliderShapeConvexHullFactory : AssetFactory<ColliderShapeAsset>
    {
        public static ColliderShapeAsset Create()
        {
            return new ColliderShapeAsset { ColliderShapes = { new ConvexHullColliderShapeDesc() } };
        }

        public override ColliderShapeAsset New()
        {
            return Create();
        }
    }

    public class ColliderShapeCylinderFactory : AssetFactory<ColliderShapeAsset>
    {
        public static ColliderShapeAsset Create()
        {
            return new ColliderShapeAsset { ColliderShapes = { new CylinderColliderShapeDesc() } };
        }

        public override ColliderShapeAsset New()
        {
            return Create();
        }
    }

    public class ColliderShapePlaneFactory : AssetFactory<ColliderShapeAsset>
    {
        public static ColliderShapeAsset Create()
        {
            return new ColliderShapeAsset { ColliderShapes = { new StaticPlaneColliderShapeDesc() } };
        }

        public override ColliderShapeAsset New()
        {
            return Create();
        }
    }

    public class ColliderShapeSphereFactory : AssetFactory<ColliderShapeAsset>
    {
        public static ColliderShapeAsset Create()
        {
            return new ColliderShapeAsset { ColliderShapes = { new SphereColliderShapeDesc() } };
        }

        public override ColliderShapeAsset New()
        {
            return Create();
        }
    }
}
