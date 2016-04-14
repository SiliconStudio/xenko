using System;
using SiliconStudio.Assets;
using SiliconStudio.Xenko.Assets.Audio;
using SiliconStudio.Xenko.Assets.Effect;
using SiliconStudio.Xenko.Assets.Entities;
using SiliconStudio.Xenko.Assets.Materials;
using SiliconStudio.Xenko.Assets.RenderFrames;
using SiliconStudio.Xenko.Assets.Skyboxes;
using SiliconStudio.Xenko.Assets.Textures;
using SiliconStudio.Xenko.Assets.UI;
using SiliconStudio.Xenko.UI;

namespace SiliconStudio.Xenko.Assets
{
    public class DefaultEffectShaderFactory : DefaultAssetFactory<EffectShaderAsset>
    {
    }

    public class DefaultMaterialFactory : DefaultAssetFactory<MaterialAsset>
    {
    }

    public class DefaultPrefabFactory : DefaultAssetFactory<PrefabAsset>
    {
    }

    public class DefaultRawAssetFactory : DefaultAssetFactory<RawAsset>
    {
    }

    public class DefaultRenderFrameFactory : DefaultAssetFactory<RenderFrameAsset>
    {
    }

    public class DefaultSceneFactory : DefaultAssetFactory<SceneAsset>
    {
    }

    public class DefaultSkyboxFactory : DefaultAssetFactory<SkyboxAsset>
    {
    }

    public class DefaultSoundEffectFactory : DefaultAssetFactory<SoundEffectAsset>
    {
    }

    public class DefaultSoundMusicFactory : DefaultAssetFactory<SoundMusicAsset>
    {
    }

    public class DefaultTextureFactory : DefaultAssetFactory<TextureAsset>
    {
    }

    public class DefaultUIFactory : AssetFactory<UIAsset>
    {
#if DEBUG
        public static UIAsset Create()
        {
            var textBlock = new Xenko.UI.Controls.TextBlock
            {
                TextSize = 60,
                Text = "Lorem Ipsum",
                TextColor = Core.Mathematics.Color.White,
            };

            var grid = new Xenko.UI.Panels.Grid();
            grid.ColumnDefinitions.Add(new StripDefinition());
            grid.LayerDefinitions.Add(new StripDefinition());
            grid.RowDefinitions.Add(new StripDefinition(StripType.Auto));

            grid.Children.Add(textBlock);
            return new UIAsset { RootElement = grid };
        }
#else
        public static UIAsset Create()
        {
            var grid = new Xenko.UI.Panels.Grid();
            grid.ColumnDefinitions.Add(new StripDefinition());
            grid.LayerDefinitions.Add(new StripDefinition());
            grid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
        
            return new UIAsset { RootElement = grid };
        }
#endif // DEBUG

        public override UIAsset New()
        {
            return Create();
        }
    }
}
