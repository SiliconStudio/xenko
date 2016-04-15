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
        //*/
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

        /*/
            var xenkoSampleTextBlock = new Xenko.UI.Controls.ContentDecorator
            {
                Name = "XenkoSamples_TextBlock_ContentDecorator",
                Content = new Xenko.UI.Controls.TextBlock
                {
                    Name = "XenkoSamples_TextBlock",
                    TextSize = 60,
                    Text = "Xenko Samples",
                    TextColor = Core.Mathematics.Color.White,
                },
                Padding = new Thickness(35, 15, 35, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            xenkoSampleTextBlock.SetPanelZIndex(1);

            // Create and initialize "UI" Text
            var uiTextBlock = new Xenko.UI.Controls.ContentDecorator
            {
                Name = "UI_TextBlock_ContentDecorator",
                Content = new Xenko.UI.Controls.TextBlock
                {
                    Name = "UI_TextBlock",
                    TextSize = 60,
                    Text = "UI",
                    TextColor = Core.Mathematics.Color.White,
                },
                Padding = new Thickness(15, 4, 15, 7),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            uiTextBlock.SetPanelZIndex(1);
            uiTextBlock.SetGridRow(1);

            // Create and initialize Xenko Logo
            var xenkoLogoImageElement = new Xenko.UI.Controls.ImageElement
            {
                Name = "XenkoLogo_ImageElement",
                HorizontalAlignment = HorizontalAlignment.Center
            };

            xenkoLogoImageElement.SetPanelZIndex(1);
            xenkoLogoImageElement.SetGridRow(3);

            // Create and initialize "Touch Screen to Start"
            var touchStartLabel = new Xenko.UI.Controls.ContentDecorator
            {
                Name = "TouchStart_TextBlock_ContentDecorator",
                Content = new Xenko.UI.Controls.TextBlock
                {
                    Name = "TouchStart_TextBlock",
                    TextSize = 42,
                    Text = "Touch Screen to Start",
                    TextColor = Core.Mathematics.Color.White
                },
                Padding = new Thickness(30, 20, 30, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            touchStartLabel.SetPanelZIndex(1);
            touchStartLabel.SetGridRow(5);

            var grid = new Xenko.UI.Panels.Grid
            {
                Name = "Grid",
                MaximumWidth = 600,
                MaximumHeight = 900,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            grid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Star, 2));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Star, 2f));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Star, 2));
            grid.RowDefinitions.Add(new StripDefinition(StripType.Auto));
            grid.ColumnDefinitions.Add(new StripDefinition());
            grid.LayerDefinitions.Add(new StripDefinition());

            grid.Children.Add(xenkoSampleTextBlock);
            grid.Children.Add(uiTextBlock);
            grid.Children.Add(xenkoLogoImageElement);
            grid.Children.Add(touchStartLabel);

            // Add the background
            var background = new Xenko.UI.Controls.ImageElement
            {
                Name = "Background_ImageElement",
                StretchType = StretchType.Fill
            };
            background.SetPanelZIndex(-1);
        //*/

            return new UIAsset
            {
                RootElement = grid
            };
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
