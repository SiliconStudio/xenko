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
                FontType = new SpriteFontTypeStatic()
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

    public class DynamicSpriteFontFactory : AssetFactory<SpriteFontAsset>
    {
        public static SpriteFontAsset Create()
        {
            return new SpriteFontAsset
            {
                FontSource = new SystemFontProvider("Arial"),
                FontType = new SpriteFontTypeDynamic(),
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
                FontType = new SpriteFontTypeSignedDistanceField()
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
