using SiliconStudio.Assets;

namespace SiliconStudio.Xenko.Assets.Sprite
{
    public class SpriteSheetSprite2DFactory : AssetFactory<SpriteSheetAsset>
    {
        public static SpriteSheetAsset Create()
        {
            return new SpriteSheetAsset
            {
                Type = SpriteSheetType.Sprite2D,
            };
        }

        public override SpriteSheetAsset New()
        {
            return Create();
        }
    }

    public class SpriteSheetUIFactory : AssetFactory<SpriteSheetAsset>
    {
        public static SpriteSheetAsset Create()
        {
            return new SpriteSheetAsset
            {
                Type = SpriteSheetType.UI,
            };
        }

        public override SpriteSheetAsset New()
        {
            return Create();
        }
    }

}
