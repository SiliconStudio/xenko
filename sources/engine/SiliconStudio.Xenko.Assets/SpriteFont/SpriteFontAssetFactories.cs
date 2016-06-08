using SiliconStudio.Assets;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public class StaticSpriteFontFactory : AssetFactory<SpriteFontAsset>
    {
        public static SpriteFontAsset Create()
        {
            return new SpriteFontAsset
            {
                FontSource = new SystemFontProvider("Arial"),
                FontType = SpriteFontType.Static,
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
                FontSource = new SystemFontProvider("Arial"),
                FontType = SpriteFontType.Dynamic,
            };
        }

        public override SpriteFontAsset New()
        {
            return Create();
        }
    }

    public class SignedDistanceFieldSpriteFontFactory: AssetFactory<SpriteFontAsset>
    {
        public static SpriteFontAsset Create()
        {
            return new SpriteFontAsset
            {
                FontSource = new SystemFontProvider("Arial"),
                FontType = SpriteFontType.SDF,
                CharacterRegions = { new CharacterRegion(' ', (char)127) }
            };
        }

        public override SpriteFontAsset New()
        {
            return Create();
        }
    }
}
