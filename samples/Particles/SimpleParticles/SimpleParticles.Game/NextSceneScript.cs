using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;

namespace SimpleParticles
{
    public class NextSceneScript : SyncScript
    {
        public string NextScene;

        public override void Update()
        {
            if (NextScene.IsNullOrEmpty())
                return;

            if (!Input.KeyDown.Contains(Keys.Right))
                return;

            Input.KeyDown.RemoveAll(x => x == Keys.Right);

            // Next scene
            SceneSystem.SceneInstance.Scene = Content.Load<Scene>(NextScene);
            Cancel();
        }
    }
}
