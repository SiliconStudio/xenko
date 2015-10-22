// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.OS;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Starter;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Graphics.Regression
{
    [Activity]
    public class AndroidGameTestActivity : AndroidXenkoActivity
    {
        public static Queue<GameBase> GamesToStart = new Queue<GameBase>();

        public static event EventHandler<EventArgs> Destroyed;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (Game == null) // application can be restarted
            {
                lock (GamesToStart)
                {
                    Game = (Game)GamesToStart.Dequeue();
                    GameTester.Logger.Info("Dequeued game '{0}'", Game.Name);
                }

                Game.Exiting += Game_Exiting;
            }

            Game.Run(GameContext);
        }

        public override void OnBackPressed()
        {
            Game.Exit();
            base.OnBackPressed();
        }

        void Game_Exiting(object sender, EventArgs e)
        {
            Finish();
        }

        protected override void OnDestroy()
        {
            Game?.Dispose();

            base.OnDestroy();

            var handler = Destroyed;
            handler?.Invoke(this, EventArgs.Empty);
        }
    }
}
#endif