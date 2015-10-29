// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using Android.App;
using Android.OS;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Starter;

namespace SiliconStudio.Xenko.Graphics.Regression
{
    [Activity]
    public class AndroidGameTestActivity : AndroidXenkoActivity
    {
        public static Game GameToStart;

        public static event EventHandler<EventArgs> Destroyed;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (Game == null) // application can be restarted
            {
                Game = GameToStart;
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