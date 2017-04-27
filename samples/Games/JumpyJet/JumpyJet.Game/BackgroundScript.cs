// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Threading.Tasks;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Engine.Events;
using SiliconStudio.Xenko.Physics;
using SiliconStudio.Xenko.Rendering.Compositing;

namespace JumpyJet
{
    public class BackgroundScript : AsyncScript
    {
        private EventReceiver gameOverListener = new EventReceiver(GameGlobals.GameOverEventKey);
        private EventReceiver gameResetListener = new EventReceiver(GameGlobals.GameResetEventKey);

        public override async Task Execute()
        {
            // Find our JumpyJetRenderer to start/stop parallax background
            var renderer = (JumpyJetRenderer)((SceneCameraRenderer)SceneSystem.GraphicsCompositor.Game).Child;

            while (Game.IsRunning)
            {
                await gameOverListener.ReceiveAsync();
                renderer.StopScrolling();

                await gameResetListener.ReceiveAsync();
                renderer.StartScrolling();
            }
        }
    }
}
