// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Threading.Tasks;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;

namespace ParticlesSample
{
    public class GameProfiler : AsyncScript
    {
        public override async Task Execute()
        {
            var game = (Game)Game;
            var enabled = false;

            while (Game.IsRunning)
            {
                if (Input.IsKeyDown(Keys.LeftShift) && Input.IsKeyDown(Keys.LeftCtrl) && Input.IsKeyReleased(Keys.P))
                {
                    if (enabled)
                    {
                        game.ProfilerSystem.DisableProfiling();
                        enabled = false;
                    }
                    else
                    {
                        game.ProfilerSystem.EnableProfiling();
                        enabled = true;
                    }
                }

                await Script.NextFrame();
            }
        }
    }
}
