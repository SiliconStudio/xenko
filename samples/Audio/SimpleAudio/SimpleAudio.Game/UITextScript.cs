using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace SimpleAudio
{
    /// <summary>
    /// The script in charge displaying the sample UI.
    /// </summary>
    public class UITextScript : StartupScript
    {
        /// <summary>
        /// The text to display on the screen.
        /// </summary>
        public string UIText = "Tap on the screen!";

        public SpriteFont Font;

        public override void Start()
        {
            base.Start();

            var textBlock = new TextBlock { TextColor = Color.White, Font = Font, Text = UIText };
            textBlock.SetCanvasPinOrigin(new Vector3(1, 0, 0));
            textBlock.SetCanvasRelativePosition(new Vector3(0.63f, 0.8f, 0f));

            Entity.Get<UIComponent>().RootElement = new Canvas { Children = { textBlock } };
        }
    }
}
