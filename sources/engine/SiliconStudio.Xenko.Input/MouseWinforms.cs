// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;
using System.Windows.Forms;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using Point = System.Drawing.Point;

namespace SiliconStudio.Xenko.Input
{
    public class MouseWinforms : MouseDeviceBase
    {
        public override string DeviceName => "Windows Mouse";
        public override Guid Id => new Guid("699e35c5-c363-4bb0-8e8b-0474ea1a5cf1");
        public override bool IsMousePositionLocked => isMousePositionLocked;
        public override PointerType Type => PointerType.Mouse;
        
        private readonly GameBase game;
        private readonly Control uiControl;
        private bool isMousePositionLocked;
        private bool wasMouseVisibleBeforeCapture;
        private Point capturedPosition;

        // Stored position for SetMousePosition
        private Point targetMousePosition;
        private bool shouldSetMousePosition;

        public MouseWinforms(GameBase game, Control uiControl)
        {
            this.game = game;
            this.uiControl = uiControl;

            uiControl.GotFocus += OnGotFocus;
            uiControl.LostFocus += OnLostFocus;
            uiControl.MouseMove += OnMouseMove;
            uiControl.MouseDown += OnMouseDown;
            uiControl.MouseUp += OnMouseUp;
            uiControl.MouseWheel += OnMouseWheelEvent;
            uiControl.MouseCaptureChanged += OnLostMouseCaptureWinForms;
            uiControl.SizeChanged += OnSizeChanged;

            OnSizeChanged(this, null);
        }

        public override void Dispose()
        {
            base.Dispose();
            uiControl.GotFocus -= OnGotFocus;
            uiControl.LostFocus -= OnLostFocus;
            uiControl.MouseMove -= OnMouseMove;
            uiControl.MouseDown -= OnMouseDown;
            uiControl.MouseUp -= OnMouseUp;
            uiControl.MouseWheel -= OnMouseWheelEvent;
            uiControl.MouseCaptureChanged -= OnLostMouseCaptureWinForms;
            uiControl.SizeChanged -= OnSizeChanged;
        }

        public override void Update()
        {
            base.Update();

            // Set mouse position
            if (shouldSetMousePosition)
            {
                Cursor.Position = targetMousePosition;
                shouldSetMousePosition = false;
            }
        }

        public override void SetMousePosition(Vector2 normalizedPosition)
        {
            Vector2 position = normalizedPosition*SurfaceSize;

            // Store setting of mouse position since it will keep the message loop goining infinitely otherwise
            var targetPoint = new Point((int)position.X, (int)position.Y);
            targetMousePosition = uiControl.PointToScreen(targetPoint);
            shouldSetMousePosition = true;
        }

        public override void LockMousePosition(bool forceCenter = false)
        {
            if (!isMousePositionLocked)
            {
                wasMouseVisibleBeforeCapture = game.IsMouseVisible;
                game.IsMouseVisible = false;
                if (forceCenter)
                {
                    capturedPosition = uiControl.PointToScreen(new Point(uiControl.ClientSize.Width/2, uiControl.ClientSize.Height/2));
                }
                capturedPosition = Cursor.Position;
                isMousePositionLocked = true;
            }
        }

        public override void UnlockMousePosition()
        {
            if (isMousePositionLocked)
            {
                isMousePositionLocked = false;
                capturedPosition = System.Drawing.Point.Empty;
                game.IsMouseVisible = wasMouseVisibleBeforeCapture;
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (isMousePositionLocked)
            {
                // Register mouse delta and reset
                HandleMoveDelta(new Vector2(Cursor.Position.X - capturedPosition.X, Cursor.Position.Y - capturedPosition.Y));
                targetMousePosition = capturedPosition;
                shouldSetMousePosition = true;
            }
            else
            {
                HandleMove(new Vector2(e.X, e.Y));
            }
        }

        private void OnSizeChanged(object sender, EventArgs eventArgs)
        {
            SetSurfaceSize(new Vector2(uiControl.ClientSize.Width, uiControl.ClientSize.Height));
        }

        private void OnMouseWheelEvent(object sender, MouseEventArgs mouseEventArgs)
        {
            HandleMouseWheel(mouseEventArgs.Delta);
        }

        private void OnMouseUp(object sender, MouseEventArgs mouseEventArgs)
        {
            HandleButtonUp(ConvertMouseButton(mouseEventArgs.Button));
        }

        private void OnMouseDown(object sender, MouseEventArgs mouseEventArgs)
        {
            uiControl.Focus();
            HandleButtonDown(ConvertMouseButton(mouseEventArgs.Button));
        }
        private void OnLostMouseCaptureWinForms(object sender, EventArgs args)
        {
            // TODO: Replace original functionality this had
            // On windows forms, the controls capture of the mouse button events at the first button pressed and release them at the first button released.
            // This has for consequence that all up-events of button simultaneously pressed are lost after the release of first button (if outside of the window).
            // This function fix the problem by forcing the mouse event capture if any mouse buttons are still down at the first button release.

            //            foreach (MouseButton button in Enum.GetValues(typeof(MouseButton)))
            //            {
            //                var buttonId = (int)button;
            //                if (MouseButtonCurrentlyDown[buttonId])
            //                    UiControl.Capture = true;
            //            }
        }

        private void OnGotFocus(object sender, EventArgs args)
        {
            //lock (KeyboardInputEvents)
            //{
            //    foreach (var key in WinKeys.mapKeys)
            //    {
            //        var state = Win32Native.GetKeyState((int)key.Key);
            //        if ((state & 0x8000) == 0x8000)
            //            KeyboardInputEvents.Add(new KeyboardWinforms.KeyboardInputEvent { Key = key.Value, Type = KeyboardWinforms.InputEventType.Down, OutOfFocus = true });
            //    }
            //}
            //LostFocus = false;
        }

        private void OnLostFocus(object sender, EventArgs args)
        {
            //LostFocus = true;
        }

        private static MouseButton ConvertMouseButton(MouseButtons mouseButton)
        {
            switch (mouseButton)
            {
                case MouseButtons.Left:
                    return MouseButton.Left;
                case MouseButtons.Right:
                    return MouseButton.Right;
                case MouseButtons.Middle:
                    return MouseButton.Middle;
                case MouseButtons.XButton1:
                    return MouseButton.Extended1;
                case MouseButtons.XButton2:
                    return MouseButton.Extended2;
            }
            return (MouseButton)(-1);
        }
    }
}
#endif