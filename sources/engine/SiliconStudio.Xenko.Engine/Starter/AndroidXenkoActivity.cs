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
using OpenTK.Graphics;
using OpenTK.Platform.Android;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Games.Android;
using SiliconStudio.Xenko.Graphics.OpenGL;

namespace SiliconStudio.Xenko.Starter
{
    // NOTE: the class should implement View.IOnSystemUiVisibilityChangeListener but doing so will prevent the engine to work on Android below 3.0 (API Level 11 is mandatory).
    // So the methods are implemented but the class does not implement View.IOnSystemUiVisibilityChangeListener.
    // Maybe this will change when support for API Level 10 is dropped
    // TODO: make this class implement View.IOnSystemUiVisibilityChangeListener when support of Android < 3.0 is dropped.
    public class AndroidXenkoActivity : Activity
    {
        private AndroidXenkoGameView gameView;

        /// <summary>
        /// The game context of the game instance.
        /// </summary>
        protected GameContextAndroid GameContext;

        /// <summary>
        /// The instance of the game to run.
        /// </summary>
        protected Game Game;

        private Action setFullscreenViewCallback;
        private StatusBarVisibility lastVisibility;
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
            gameView = new AndroidXenkoGameView(this);

            // setup the application view and xenko game context
            SetupGameViewAndGameContext();

            // set up a listener to the android ringer mode (Normal/Silent/Vibrate)
            ringerModeIntentReceiver = new RingerModeIntentReceiver((AudioManager)GetSystemService(AudioService));
            RegisterReceiver(ringerModeIntentReceiver, new IntentFilter(AudioManager.RingerModeChangedAction));

            SetFullscreenView();
            InitializeFullscreenViewCallback();
        }

        public void OnSystemUiVisibilityChange(StatusBarVisibility visibility)
        {
            //Log.Debug("Xenko", "OnSystemUiVisibilityChange: visibility=0x{0:X8}", (int)visibility);
            var diffVisibility = lastVisibility ^ visibility;
            lastVisibility = visibility;
            if ((((int)diffVisibility & (int)SystemUiFlags.LowProfile) != 0) && (((int)visibility & (int)SystemUiFlags.LowProfile) == 0))
            {
                // visibility has changed out of low profile mode; change it back, which requires a delay to work properly:
                // http://stackoverflow.com/questions/11027193/maintaining-lights-out-mode-view-setsystemuivisibility-across-restarts
                RemoveFullscreenViewCallback();
                PostFullscreenViewCallback();
            }
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            //Log.Debug("Xenko", "OnWindowFocusChanged: hasFocus={0}", hasFocus);
            base.OnWindowFocusChanged(hasFocus);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                // use fullscreen immersive mode
                if (hasFocus)
                {
                    SetFullscreenView();
                }
            }
            // TODO: uncomment this once the class implements View.IOnSystemUiVisibilityChangeListener.
            /*else if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
            {
                // use fullscreen low profile mode, with a delay
                if (hasFocus)
                {
                    RemoveFullscreenViewCallback();
                    PostFullscreenViewCallback();
                }
                else
                {
                    RemoveFullscreenViewCallback();
                }
            }*/
        }

        private void SetupGameViewAndGameContext()
        {
            // Set the main view of the Game
            SetContentView(Resource.Layout.Game);
            mainLayout = FindViewById<RelativeLayout>(Resource.Id.GameViewLayout);
            mainLayout.AddView(gameView);

            // Create the Game context
            GameContext = new GameContextAndroid(gameView, FindViewById<RelativeLayout>(Resource.Id.EditTextLayout));
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

        private void InitializeFullscreenViewCallback()
        {
            //Log.Debug("Xenko", "InitializeFullscreenViewCallback");
            if ((Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich) && (Build.VERSION.SdkInt < BuildVersionCodes.Kitkat))
            {
                setFullscreenViewCallback = SetFullscreenView;
                // TODO: uncomment this once the class implements View.IOnSystemUiVisibilityChangeListener. Right now only Kitkat supports full screen    
                //Window.DecorView.SetOnSystemUiVisibilityChangeListener(this);
            }
        }

        private void PostFullscreenViewCallback()
        {
            //Log.Debug("Xenko", "PostFullscreenViewCallback");
            var handler = Window.DecorView.Handler;
            if (handler != null)
            {
                // post callback with delay, which needs to be longer than transient status bar timeout, otherwise it will have no effect!
                handler.PostDelayed(setFullscreenViewCallback, 4000);
            }
        }

        private void RemoveFullscreenViewCallback()
        {
            //Log.Debug("Xenko", "RemoveFullscreenViewCallback");
            var handler = Window.DecorView.Handler;
            if (handler != null)
            {
                // remove any pending callbacks
                handler.RemoveCallbacks(setFullscreenViewCallback);
            }
        }

        private void SetFullscreenView()
        {
            //Log.Debug("Xenko", "SetFullscreenView");
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich) // http://redth.codes/such-android-api-levels-much-confuse-wow/
            {
                var view = Window.DecorView;
                int flags = (int)view.SystemUiVisibility;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBean)
                {
                    // http://developer.android.com/training/system-ui/status.html
                    flags |= (int)(SystemUiFlags.Fullscreen | SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutStable);
                }
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
                {
                    // http://developer.android.com/training/system-ui/immersive.html; the only mode that can really hide the nav bar
                    flags |= (int)(SystemUiFlags.HideNavigation | SystemUiFlags.ImmersiveSticky | SystemUiFlags.LayoutHideNavigation);
                }
                else
                {
                    // http://developer.android.com/training/system-ui/dim.html; low profile or 'lights out' mode to minimize the nav bar
                    flags |= (int)SystemUiFlags.LowProfile;
                }
                view.SystemUiVisibility = (StatusBarVisibility)flags;
            }
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
