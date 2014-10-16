// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_ANDROID
using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Media;
using OpenTK.Platform.Android;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Games.Android;
using SiliconStudio.Paradox.UI;

namespace SiliconStudio.Paradox.Starter
{
    public class AndroidParadoxActivity : Activity, View.IOnTouchListener
    {
        private AndroidGameView gameView;

        /// <summary>
        /// The game context of the game instance.
        /// </summary>
        protected GameContext GameContext;

        /// <summary>
        /// The instance of the game to run.
        /// </summary>
        protected Game Game;

        private RelativeLayout mainLayout;
        private RingerModeIntentReceiver ringerModeIntentReceiver;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set the android global context
            if (PlatformAndroid.Context == null)
                PlatformAndroid.Context = this;

            // Set the format of the window color buffer (avoid conversions)
            // TODO: PDX-364: depth format is currently hard coded (need to investigate how it can be transmitted)
            Window.SetFormat(Format.Rgba8888);

            // Remove the title bar
            RequestWindowFeature(WindowFeatures.NoTitle);

            // Unpack the files contained in the apk
            //await VirtualFileSystem.UnpackAPK();
            
            // Create the Android OpenGl view
            gameView = new AndroidParadoxGameView(this);

            // setup the application view and paradox game context
            SetupGameViewAndGameContext();

            // set up a listener to the android ringer mode (Normal/Silent/Vibrate)
            ringerModeIntentReceiver = new RingerModeIntentReceiver((AudioManager)GetSystemService(AudioService));
            RegisterReceiver(ringerModeIntentReceiver, new IntentFilter(AudioManager.RingerModeChangedAction));
        }

        private void SetupGameViewAndGameContext()
        {
            // Set the main view of the Game
            SetContentView(Resource.Layout.Game);
            mainLayout = FindViewById<RelativeLayout>(Resource.Id.GameViewLayout);
            mainLayout.AddView(gameView);

            // Create the Game context
            GameContext = new GameContext(gameView, FindViewById<RelativeLayout>(Resource.Id.EditTextLayout));
        }

        public override void SetContentView(View view)
        {
            gameView = view as AndroidGameView;
            SetupGameViewAndGameContext();
        }

        public override void SetContentView(View view, ViewGroup.LayoutParams @params)
        {
            gameView = view as AndroidGameView;
            SetupGameViewAndGameContext();
        }

        protected override void OnPause()
        {
            base.OnPause();

            UnregisterReceiver(ringerModeIntentReceiver);

            if (gameView != null)
                gameView.Pause();
        }

        protected override void OnResume()
        {
            base.OnResume();

            RegisterReceiver(ringerModeIntentReceiver, new IntentFilter(AudioManager.RingerModeChangedAction));

            if (gameView != null)
                gameView.Resume();
        }

        public bool OnTouch(View v, MotionEvent e)
        {
            throw new NotImplementedException();
        }

        private class RingerModeIntentReceiver : BroadcastReceiver
        {
            private readonly AudioManager audioManager;

            private int muteCounter;

            public RingerModeIntentReceiver(AudioManager audioManager)
            {
                this.audioManager = audioManager;
            }

            public override void OnReceive(Context context, Intent intent)
            {
                UpdateMusicMuteStatus();
            }

            private void UpdateMusicMuteStatus()
            {
                switch (audioManager.RingerMode)
                {
                    case RingerMode.Normal:
                        for (int i = 0; i < muteCounter; i++)
                            audioManager.SetStreamMute(Stream.Music, false);
                        muteCounter = 0;
                        break;
                    case RingerMode.Silent:
                    case RingerMode.Vibrate:
                        audioManager.SetStreamMute(Stream.Music, true);
                        ++muteCounter;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
#endif