// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if (SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_UNIX) && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL && SILICONSTUDIO_XENKO_UI_OPENTK
using System;
using OpenTK.Input;
using SiliconStudio.Xenko.Games;
using GameWindow = OpenTK.GameWindow;
using Vector2 = SiliconStudio.Core.Mathematics.Vector2;

namespace SiliconStudio.Xenko.Input
{
    public class MouseOpenTK : MouseDeviceBase
    {
        public override string DeviceName => "OpenTK Mouse";
        public override Guid Id => new Guid("b9f9fd0c-b090-4826-9d6b-c1118bb7c2d0");
        public override bool IsMousePositionLocked => isMousePositionLocked;

        private GameWindow gameWindow;
        private readonly GameBase game;
        private bool isMousePositionLocked;
        private bool wasMouseVisibleBeforeCapture;
        private Vector2 capturedPosition;

        public MouseOpenTK(GameBase game, GameWindow gameWindow)
        {
            this.game = game;
            this.gameWindow = gameWindow;
            gameWindow.MouseDown += Mouse_ButtonDown;
            gameWindow.MouseUp += Mouse_ButtonUp;
            gameWindow.MouseMove += Mouse_Move;
            gameWindow.MouseWheel += Mouse_Wheel;
            gameWindow.Resize += GameWindowOnResize;
            GameWindowOnResize(null, EventArgs.Empty);
        }

        public override void Dispose()
        {
            gameWindow.MouseDown -= Mouse_ButtonDown;
            gameWindow.MouseUp -= Mouse_ButtonUp;
            gameWindow.MouseMove -= Mouse_Move;
            gameWindow.MouseWheel -= Mouse_Wheel;
            gameWindow.Resize -= GameWindowOnResize;
        }

        public override void LockMousePosition(bool forceCenter)
        {
            if (!isMousePositionLocked)
            {
                wasMouseVisibleBeforeCapture = game.IsMouseVisible;
                game.IsMouseVisible = false;
                if (forceCenter)
                {
                    SetMousePosition(new Vector2(0.5f, 0.5f));
                }
                var mouseState = Mouse.GetState();
                capturedPosition = new Vector2(mouseState.X, mouseState.Y);
                isMousePositionLocked = true;
            }
        }

        public override void UnlockMousePosition()
        {
            if (isMousePositionLocked)
            {
                isMousePositionLocked = false;
                capturedPosition = new Vector2();
                game.IsMouseVisible = wasMouseVisibleBeforeCapture;
            }
        }

        public override void SetMousePosition(Vector2 normalizedPosition)
        {
            Vector2 position = normalizedPosition*SurfaceSize;
            Mouse.SetPosition(position.X, position.Y);
        }

        private void GameWindowOnResize(object sender, EventArgs eventArgs)
        {
            SetSurfaceSize(new Vector2(gameWindow.Width, gameWindow.Height));
        }
        
        private void Mouse_Wheel(object sender, MouseWheelEventArgs e)
        {
            HandleMouseWheel(e.Delta);
        }

        private void Mouse_Move(object sender, MouseMoveEventArgs e)
        {
            if (isMousePositionLocked)
            {
                // Register mouse delta and reset
                var cursorState = Mouse.GetCursorState();
                HandleMoveDelta(new Vector2(cursorState.X - capturedPosition.X, cursorState.Y - capturedPosition.Y));
                Mouse.SetPosition(capturedPosition.X, capturedPosition.Y);
            }
            else
            {
                HandleMove(new Vector2(e.Position.X, e.Position.Y));
            }
        }

        private void Mouse_ButtonUp(object sender, MouseButtonEventArgs e)
        {
            HandleButtonUp(ConvertMouseButtonFromOpenTK(e.Button));
        }

        private void Mouse_ButtonDown(object sender, MouseButtonEventArgs e)
        {
            HandleButtonDown(ConvertMouseButtonFromOpenTK(e.Button));
        }

        private MouseButton ConvertMouseButtonFromOpenTK(OpenTK.Input.MouseButton mouseButton)
        {
            switch (mouseButton)
            {
                case OpenTK.Input.MouseButton.Left:
                    return MouseButton.Left;
                case OpenTK.Input.MouseButton.Right:
                    return MouseButton.Right;
                case OpenTK.Input.MouseButton.Middle:
                    return MouseButton.Middle;
                case OpenTK.Input.MouseButton.Button1:
                    return MouseButton.Extended1;
                case OpenTK.Input.MouseButton.Button2:
                    return MouseButton.Extended2;
            }
            return (MouseButton)(-1);
        }
    }
}

#endif