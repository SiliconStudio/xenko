using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace GameMenu
{
    public class SplashScript : UISceneBase
    {
        public SpriteFont WesternFont;
        public SpriteSheet SplashScreenImages;

        protected override void LoadScene()
        {
            // Allow user to resize the window with the mouse.
            Game.Window.AllowUserResizing = true;

            // Create and initialize "Xenko Samples" Text
            var xenkoSampleTextBlock = new ContentDecorator
            {
                BackgroundImage = SpriteFromSheet.Create(SplashScreenImages, "xenko_sample_text_bg"),
                Content = new TextBlock
                {
                    Font = WesternFont,
                    TextSize = 60,
                    Text = "Xenko Samples",
                    TextColor = Color.White,
                },
                Padding = new Thickness(35, 15, 35, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            xenkoSampleTextBlock.SetPanelZIndex(1);

            // Create and initialize "UI" Text
            var uiTextBlock = new ContentDecorator
            {
                BackgroundImage = SpriteFromSheet.Create(SplashScreenImages, "ui_text_bg"),
                Content = new TextBlock
                {
                    Font = WesternFont,
                    TextSize = 60,
                    Text = "UI",
                    TextColor = Color.White,
                },
                Padding = new Thickness(15, 4, 15, 7),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            uiTextBlock.SetPanelZIndex(1);
            uiTextBlock.SetGridRow(1);

            // Create and initialize Xenko Logo
            var xenkoLogoImageElement = new ImageElement
            {
                Source = SpriteFromSheet.Create(SplashScreenImages, "Logo"),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            xenkoLogoImageElement.SetPanelZIndex(1);
            xenkoLogoImageElement.SetGridRow(3);

            // Create and initialize "Touch Screen to Start"
            var touchStartLabel = new ContentDecorator
            {
                BackgroundImage = SpriteFromSheet.Create(SplashScreenImages, "touch_start_frame"),
                Content = new TextBlock
                {
                    Font = WesternFont,
                    TextSize = 42,
                    Text = "Touch Screen to Start",
                    TextColor = Color.White
                },
                Padding = new Thickness(30, 20, 30, 25),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            touchStartLabel.SetPanelZIndex(1);
            touchStartLabel.SetGridRow(5);

            var grid = new Grid
            {
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
            var background = new ImageElement { Source = SpriteFromSheet.Create(SplashScreenImages, "background_uiimage"), StretchType = StretchType.Fill };
            background.SetPanelZIndex(-1);

            Entity.Get<UIComponent>().RootElement = new UniformGrid { Children = { background, grid } };
        }

        protected override void UpdateScene()
        {
            if (Input.PointerEvents.Count > 0)
            {
                // Next scene
                SceneSystem.SceneInstance.Scene = Content.Load<Scene>("MainScene");
                Cancel();
            }
        }
    }
}
