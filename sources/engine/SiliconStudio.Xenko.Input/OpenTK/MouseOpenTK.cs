// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if (SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_UNIX) && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL && SILICONSTUDIO_XENKO_UI_OPENTK
using System;
using OpenTK;
using OpenTK.Input;
using SiliconStudio.Xenko.Games;
using Point = System.Drawing.Point;
using GameWindow = OpenTK.GameWindow;
using Vector2 = SiliconStudio.Core.Mathematics.Vector2;

namespace SiliconStudio.Xenko.Input
{
    internal class MouseOpenTK : MouseDeviceBase, IDisposable
    {
        private GameWindow uiControl;
        private readonly GameBase game;
        private bool isMousePositionLocked;
        private bool wasMouseVisibleBeforeCapture;
        private Point capturedPosition;

        public MouseOpenTK(GameBase game, GameWindow uiControl)
        {
            this.game = game;
            this.uiControl = uiControl;
            uiControl.MouseDown += Mouse_ButtonDown;
            uiControl.MouseUp += Mouse_ButtonUp;
            uiControl.MouseMove += Mouse_Move;
            uiControl.MouseWheel += Mouse_Wheel;
            uiControl.Resize += GameWindowOnResize;
            GameWindowOnResize(null, EventArgs.Empty);
        }

        public void Dispose()
        {
            uiControl.MouseDown -= Mouse_ButtonDown;
            uiControl.MouseUp -= Mouse_ButtonUp;
            uiControl.MouseMove -= Mouse_Move;
            uiControl.MouseWheel -= Mouse_Wheel;
            uiControl.Resize -= GameWindowOnResize;
        }

        public override string Name => "OpenTK Mouse";
        public override Guid Id => new Guid("b9f9fd0c-b090-4826-9d6b-c1118bb7c2d0");
        public override bool IsPositionLocked => isMousePositionLocked;

        public override void LockPosition(bool forceCenter)
        {
            if (!isMousePositionLocked)
            {
                wasMouseVisibleBeforeCapture = game.IsMouseVisible;
                //game.IsMouseVisible = false;
                if (forceCenter)
                {
                    SetPosition(new Vector2(0.5f, 0.5f));
                }
                var mouseState = Mouse.GetCursorState();
                capturedPosition = uiControl.PointToClient(new Point(mouseState.X, mouseState.Y));
                isMousePositionLocked = true;
            }
        }

        public override void UnlockPosition()
        {
            if (isMousePositionLocked)
            {
                isMousePositionLocked = false;
                game.IsMouseVisible = wasMouseVisibleBeforeCapture;
            }
        }

        public override void SetPosition(Vector2 normalizedPosition)
        {
            Vector2 position = normalizedPosition*SurfaceSize;
            
            Mouse.SetPosition(position.X, position.Y);
        }

        private void GameWindowOnResize(object sender, EventArgs eventArgs)
        {
            SetSurfaceSize(new Vector2(uiControl.Width, uiControl.Height));
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
                HandleMouseDelta(new Vector2(e.X - capturedPosition.X, e.Y - capturedPosition.Y));
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