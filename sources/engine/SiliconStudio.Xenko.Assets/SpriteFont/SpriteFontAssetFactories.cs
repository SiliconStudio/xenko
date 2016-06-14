using SiliconStudio.Assets;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Assets.SpriteFont
{
    public class OfflineRasterizedSpriteFontFactory : AssetFactory<SpriteFontAsset>
    {
        public static SpriteFontAsset Create()
        {
            return new SpriteFontAsset
            {
                FontSource = new SystemFontProvider("Arial"),
                FontType = new OfflineRasterizedSpriteFontType()
                {
                    CharacterRegions = { new CharacterRegion(' ', (char)127) }                 
                },
            };
        }

        public override SpriteFontAsset New()
        {
            return Create();
        }
    }

    public class RuntimeRasterizedSpriteFontFactory : AssetFactory<SpriteFontAsset>
    {
        public static SpriteFontAsset Create()
        {
            return new SpriteFontAsset
            {
                FontSource = new SystemFontProvider("Arial"),
                FontType = new RuntimeRasterizedSpriteFontType(),
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
                FontType = new SignedDistanceFieldSpriteFontType()
                {
                    CharacterRegions = { new CharacterRegion(' ', (char)127) }
                },
            };
        }

        public override SpriteFontAsset New()
        {
            return Create();
        }
    }
}
