using SiliconStudio.Assets;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public class StaticSpriteFontFactory : AssetFactory<SpriteFontAsset>
    {
        public static SpriteFontAsset Create()
        {
            return new SpriteFontAsset
            {
                FontName = "Arial",
                IsDynamic = false,
                CharacterRegions = { new CharacterRegion(' ', (char)127) }
            };
        }

        public override SpriteFontAsset New()
        {
            return Create();
        }
    }

    public class DynamicSpriteFontFactory : AssetFactory<SpriteFontAsset>
    {
        public static SpriteFontAsset Create()
        {
            return new SpriteFontAsset
            {
                FontName = "Arial",
                IsDynamic = true
            };
        }

        public override SpriteFontAsset New()
        {
            return Create();
        }
    }
}
