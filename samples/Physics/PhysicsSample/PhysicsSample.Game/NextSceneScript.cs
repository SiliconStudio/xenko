using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;

namespace PhysicsSample
{
    public class NextSceneScript : SyncScript
    {
        public string NextScene;

        public override void Update()
        {
            if (NextScene.IsNullOrEmpty())
                return;

            if (!Input.IsKeyPressed(Keys.Right))
                return;

            // Next scene
            SceneSystem.SceneInstance.Scene = Content.Load<Scene>(NextScene);
            Cancel();
        }
    }
}
