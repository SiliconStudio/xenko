using System;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Rendering.Sprites;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace UIElementLink
{
    public class SplashScript : UISceneBase
    {
        public SpriteSheet SplashScreenImages;

        public string NextScene;

        private Button followedButton;

        private Vector2 centerPoint;

        private void LoadNextScene()
        {
            if (NextScene.IsNullOrEmpty())
                return;

            SceneSystem.SceneInstance.Scene = Content.Load<Scene>(NextScene);
            Cancel();
        }

        protected override void LoadScene()
        {
            Game.Window.AllowUserResizing = false;

            var backBufferSize = new Vector2(GraphicsDevice.Presenter.BackBuffer.Width, GraphicsDevice.Presenter.BackBuffer.Height);
            centerPoint = new Vector2(backBufferSize.X / 2, backBufferSize.Y / 2);

            var longButton = (SpriteFromTexture) SplashScreenImages["button_long"];
            var longSize = new Vector3(SplashScreenImages["button_long"].SizeInPixels.X,
                SplashScreenImages["button_long"].SizeInPixels.Y, 0);

            // This button will be followed
            followedButton = new Button
            {
                PressedImage = longButton,
                NotPressedImage = longButton,
                MouseOverImage =  longButton,

                Size = longSize,

                // This element will be followed, because we have specified the same name in the FollowingEntity's UI Element Link
                Name = "ElementName",
            };

            // Load the next scene when the user clicks the button
            followedButton.Click += delegate { LoadNextScene(); };

            // Corner buttons
            var boxButton = (SpriteFromTexture)SplashScreenImages["button_box"];
            var boxSize = new Vector3(SplashScreenImages["button_box"].SizeInPixels.X,
                SplashScreenImages["button_box"].SizeInPixels.Y, 0);

            var cornerTL = new Button { PressedImage = boxButton, NotPressedImage = boxButton, MouseOverImage = boxButton, Size = boxSize };
            cornerTL.SetCanvasAbsolutePosition(new Vector3(0, 0, 0));

            var cornerTR = new Button { PressedImage = boxButton, NotPressedImage = boxButton, MouseOverImage = boxButton, Size = boxSize };
            cornerTR.SetCanvasAbsolutePosition(new Vector3(backBufferSize.X - boxSize.X, 0, 0));

            var cornerBL = new Button { PressedImage = boxButton, NotPressedImage = boxButton, MouseOverImage = boxButton, Size = boxSize };
            cornerBL.SetCanvasAbsolutePosition(new Vector3(0, backBufferSize.Y - boxSize.Y, 0));

            var cornerBR = new Button { PressedImage = boxButton, NotPressedImage = boxButton, MouseOverImage = boxButton, Size = boxSize };
            cornerBR.SetCanvasAbsolutePosition(new Vector3(backBufferSize.X - boxSize.X, backBufferSize.Y - boxSize.Y, 0));

            var rootElement = new Canvas() { Children = { followedButton, cornerTL, cornerTR, cornerBL, cornerBR },
                MaximumWidth = backBufferSize.X, MaximumHeight = backBufferSize.Y };

            Entity.Get<UIComponent>().RootElement = rootElement;
        }

        protected override void UpdateScene()
        {
            // Move the followed button around
            var distance = (float) Math.Sin(Game.UpdateTime.Total.TotalSeconds * 0.2f) * centerPoint.X * 0.75f;
            followedButton.SetCanvasAbsolutePosition(new Vector3(centerPoint.X + distance, centerPoint.Y, 0));
        }
    }
}
