using System;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace RibbonParticles
{
    public class UIScript : StartupScript
    {
        public Entity Knight;

        public SpriteFont Font;

        public override void Start()
        {
            base.Start();

            // Setup the UI
            Entity.Get<UIComponent>().RootElement = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 20, 0),
                Children = { CreateButton("Idle"), CreateButton("Run") }
            };

            // Set the default animation
            Knight.Get<AnimationComponent>().Play("Run");
        }

        /// <summary>
        /// Create a button and link the click action to the corresponding animation.
        /// </summary>
        private Button CreateButton(string animationName)
        {
            var idleButton = new Button
            {
                Content = new TextBlock
                {
                    Text = "Play " + animationName,
                    Font = Font,
                },
                Padding = new Thickness(10, 10, 10, 10),
                Margin = new Thickness(0, 0, 0, 10),
            };
            idleButton.Click += (s, e) => Knight.Get<AnimationComponent>().Crossfade(animationName, TimeSpan.FromSeconds(0.1));

            return idleButton;
        }
    }
}
