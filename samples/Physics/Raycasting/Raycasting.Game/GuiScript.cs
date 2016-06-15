using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace Raycasting
{
    /// <summary>
    /// The script in charge of displaying the UI
    /// </summary>
    public class GuiScript : StartupScript
    {
        public SpriteFont Font;

        public override void Start()
        {
            base.Start();

            var textBlock = new TextBlock
            {
                Text = "Shoot the cubes!",
                Font = Font,
                TextColor = Color.White,
                TextSize = 60
            };
            textBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 0));
            textBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.9f, 0f));

            Entity.Get<UIComponent>().RootElement = new Canvas { Children = { textBlock } };
        }
    }
}