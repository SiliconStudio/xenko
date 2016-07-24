// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
#if SILICONSTUDIO_PLATFORM_IOS
using UIKit;
using SiliconStudio.Xenko.Starter;
#endif
using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics.Regression
{
    public class GameTester
    {
        public readonly static Logger Logger = GlobalLogger.GetLogger("GameTester");

        private static object uniThreadLock = new object();

        public static void RunGameTest(Game game)
        {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP

            using (game)
            {
                game.Run();
            }

#elif SILICONSTUDIO_PLATFORM_WINDOWS_RUNTIME

            throw new NotImplementedException();

#elif SILICONSTUDIO_PLATFORM_IOS || SILICONSTUDIO_PLATFORM_ANDROID

            lock(uniThreadLock)
            {
                // Prepare finish callback
                var tcs = new TaskCompletionSource<bool>();
                EventHandler<EventArgs> gameFinishedCallback = (sender, e) =>
                {
                    // Notify waiter that game has exited
                    Logger.Info("Game finished.");
                    tcs.TrySetResult(true);
                };

                EventHandler<GameUnhandledExceptionEventArgs> exceptionhandler = (sender, e) =>
                {
                    Logger.Info("Game finished with exception ={0}.", e);
                    tcs.TrySetException((Exception)e.ExceptionObject);
                };

                // Transmit data to activity
                // TODO: Avoid static with string intent + Dictionary?
                try
                {
                    game.UnhandledException += exceptionhandler;

                    Logger.Info(@"Starting activity");
#if SILICONSTUDIO_PLATFORM_IOS
                    game.Exiting += gameFinishedCallback;

                    UIApplication.SharedApplication.InvokeOnMainThread(() =>
                    {
                        var window = UIApplication.SharedApplication.KeyWindow;
                        var rootNavigationController = (UINavigationController)window.RootViewController;

                        // create the xenko game view 
                        var bounds = UIScreen.MainScreen.Bounds;
                        var xenkoGameView = new iOSXenkoView((System.Drawing.RectangleF)bounds) { ContentScaleFactor = UIScreen.MainScreen.Scale };

                        // create the view controller used to display the xenko game
                        var xenkoGameController = new iOSGameTestController(game) { View = xenkoGameView };

                        // create the game context
                        var gameContext = new GameContextiOS(new iOSWindow(window, xenkoGameView, xenkoGameController));

                        // push view
                        rootNavigationController.PushViewController(gameContext.Control.GameViewController, false);

                        // launch the game
                        game.Run(gameContext);
                    });
#elif SILICONSTUDIO_PLATFORM_ANDROID
                    // Start activity
                    AndroidGameTestActivity.GameToStart = game;
                    AndroidGameTestActivity.Destroyed += gameFinishedCallback;
                    PlatformAndroid.Context.StartActivity(typeof(AndroidGameTestActivity));
#endif
                    // Wait for completion of task
                    // TODO: Should we put a timeout and issue a Game.Exit() in main thread if too long?
                    tcs.Task.Wait();

                    Logger.Info(@"Activity ended");
                }
                catch (AggregateException e)
                {
                    // Unwrap aggregate exceptions
                    if (e.InnerExceptions.Count == 1)
                        ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                }
                finally
                {
#if SILICONSTUDIO_PLATFORM_IOS
                    // iOS Cleanup
                    UIApplication.SharedApplication.InvokeOnMainThread(() =>
                    {
                        var window = UIApplication.SharedApplication.KeyWindow;
                        var rootNavigationController = (UINavigationController)window.RootViewController;

                        rootNavigationController.PopViewController(false);
                    });
#elif SILICONSTUDIO_PLATFORM_ANDROID
                    AndroidGameTestActivity.Destroyed -= gameFinishedCallback;
                    AndroidGameTestActivity.GameToStart = null;
#endif
                }
            }
#endif
        }
    }
}
