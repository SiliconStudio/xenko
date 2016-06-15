using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;

namespace RibbonParticles
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