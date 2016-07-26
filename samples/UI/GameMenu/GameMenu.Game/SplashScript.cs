using SiliconStudio.Xenko.Engine;

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
            if (Input.PointerEvents.Count > 0)
            {
                // Next scene
                SceneSystem.SceneInstance.Scene = Content.Load<Scene>("MainScene");
                Cancel();
            }
        }
    }
}
