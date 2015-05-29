// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PLATFORM_IOS

using System;
using CoreGraphics;
using UIKit;
using Foundation;
using OpenTK.Platform.iPhoneOS;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Input
{
    public partial class InputManager
    {
        private UIWindow window;
        private iPhoneOSGameView view;

        public InputManager(IServiceRegistry registry) : base(registry)
        {
            HasKeyboard = true;
            HasMouse = false;
            HasPointer = true;
        }

        public override void Initialize()
        {
            view = Game.Context.GameView;
            window = Game.Context.MainWindow;

            var gameController = Game.Context.GameViewController;

            window.UserInteractionEnabled = true;
            window.MultipleTouchEnabled = true;
            gameController.TouchesBeganDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            gameController.TouchesMovedDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            gameController.TouchesEndedDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            gameController.TouchesCancelledDelegate += (touchesSet, _) => HandleTouches(touchesSet);
            view.Resize += OnResize;

            OnResize(null, EventArgs.Empty);
        }

        private void OnResize(object sender, EventArgs eventArgs)
        {
            ControlHeight = (float)view.Frame.Height;
            ControlWidth = (float)view.Frame.Width;
        }

        private void HandleTouches(NSSet touchesSet)
        {
            var touches = touchesSet.ToArray<UITouch>();

            if (touches != null)
            {
                foreach (var uitouch in touches)
                {
                    var id = uitouch.Handle.ToInt32();
                    var position = NormalizeScreenPosition(CGPointToVector2(uitouch.LocationInView(view)));

                    HandlePointerEvents(id, position, GetState(uitouch));
                }
            }
        }

        private PointerState GetState(UITouch touch)
        {
            switch (touch.Phase)
            {
                case UITouchPhase.Began:
                    return PointerState.Down;
                case UITouchPhase.Moved:
                case UITouchPhase.Stationary:
                    return PointerState.Move;
                case UITouchPhase.Ended:
                    return PointerState.Up;
                case UITouchPhase.Cancelled:
                    return PointerState.Cancel;
            }

            throw new ArgumentException("Got an invalid Touch event in GetState");
        }

        private Vector2 CGPointToVector2(CGPoint point)
        {
            return new Vector2((float)point.X, (float)point.Y);
        }

        public override bool MultiTouchEnabled
        {
            get { return Game.Context.GameView.MultipleTouchEnabled; } 
            set { Game.Context.GameView.MultipleTouchEnabled = value; }
        }
    }
}
#endif