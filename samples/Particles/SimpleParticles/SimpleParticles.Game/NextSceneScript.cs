using System.Linq;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;

namespace SimpleParticles
{
    public class NextSceneScript : SyncScript
    {
        public string NextScene;

        private PointerState oldState = PointerState.Up;

        public override void Update()
        {
            if (NextScene.IsNullOrEmpty())
                return;

            if (oldState == PointerState.Up && Input.PointerEvents.Any(e => e.State == PointerState.Down))
            {
                // Next scene
                SceneSystem.SceneInstance.Scene = Content.Load<Scene>(NextScene);
                Cancel();
            }
            else
            {
                oldState = PointerState.Up;
            }
        }
    }
}
