// Copyright (c) 2014-2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Diagnostics;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Testing
{
    //This is how we inject the assembly to run automatically at game start, paired with Xenko.targets and the msbuild property SiliconStudioAutoTesting
    internal class Module
    {
        public static GameTestingSystem TestClient;

        [ModuleInitializer]
        public static void Initialize()
        {
            Game.GameStarted += (sender, args) =>
            {              
                var game = args.Game;
                TestClient = new GameTestingSystem(game.Services);
                TestClient.StartClient(game);
            };
        }
    }
}
