// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Starter;
using SiliconStudio.Paradox.Engine;

namespace SiliconStudio.Paradox.Graphics.Regression
{
    [Activity]
    public class AndroidGameTestActivity : AndroidParadoxActivity
    {
        public static Queue<GameBase> GamesToStart = new Queue<GameBase>();

        public static event EventHandler<EventArgs> Destroyed;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            lock (GamesToStart)
            {
                Game = (Game)GamesToStart.Dequeue();
            }

            Game.Exiting += Game_Exiting;
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
            if (Game != null)
                Game.Dispose();

            base.OnDestroy();

            var handler = Destroyed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
#endif