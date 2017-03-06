using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;

namespace GameMenu
{
    public class SplashScript : UISceneBase
    {
        protected override void LoadScene()
        {
            // Allow user to resize the window with the mouse.
            Game.Window.AllowUserResizing = true;
        }

        protected override void UpdateScene()
        {
            if (Input.PointerEvents.Any(e => e.EventType == PointerEventType.Pressed))
            {
                // Next scene
                SceneSystem.SceneInstance.RootScene = Content.Load<Scene>("MainScene");
                Cancel();
            }
        }
    }
}
