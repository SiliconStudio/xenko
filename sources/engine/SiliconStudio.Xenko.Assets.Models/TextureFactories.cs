using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Xenko.Assets.Textures;

namespace SiliconStudio.Xenko.Assets.Models
{
    public class ColorTextureFactory : AssetFactory<TextureAsset>
    {
        public static TextureAsset Create()
        {
            return new TextureAsset { Type = new ColorTextureType() };
        }

        public override TextureAsset New()
        {
            return Create();
        }
    }

    public class NormalMapTextureFactory : AssetFactory<TextureAsset>
    {
        public static TextureAsset Create()
        {
            return new TextureAsset { Type = new NormapMapTextureType() };
        }

        public override TextureAsset New()
        {
            return Create();
        }
    }

    public class GrayscaleTextureFactory : AssetFactory<TextureAsset>
    {
        public static TextureAsset Create()
        {
            return new TextureAsset { Type = new GrayscaleTextureType() };
        }

        public override TextureAsset New()
        {
            return Create();
        }
    }
}
