// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input
{
    public class MouseSimulated : MouseDeviceBase
    {
        private bool positionLocked;
        private Vector2 capturedPosition;

        public MouseSimulated(InputSourceSimulated source)
        {
            Priority = -1000;
            SetSurfaceSize(Vector2.One);
            Source = source;
        }

        public override string Name => "Simulated Mouse";

        public override Guid Id => new Guid(10, 10, 2, 0, 0, 0, 0, 0, 0, 0, 0);

        public override bool IsPositionLocked => positionLocked;

        public override IInputSource Source { get; }

        public new MouseDeviceState MouseState => base.MouseState;
        public new PointerDeviceState PointerState => base.PointerState;

        public override void Update(List<InputEvent> inputEvents)
        {
            base.Update(inputEvents);

            if (positionLocked)
            {
                MouseState.Position = capturedPosition;
                PointerState.GetPointerData(0).Position = capturedPosition;
            }
        }

        public void SimulateMouseDown(MouseButton button)
        {
            MouseState.HandleButtonDown(button);
        }

        public void SimulateMouseUp(MouseButton button)
        {
            MouseState.HandleButtonUp(button);
        }

        public void SimulateMouseWheel(float wheelDelta)
        {
            MouseState.HandleMouseWheel(wheelDelta);
        }

        public override void SetPosition(Vector2 position)
        {
            if (IsPositionLocked)
            {
                MouseState.HandleMouseDelta(position * SurfaceSize - capturedPosition);
            }
            else
            {
                MouseState.HandleMove(position * SurfaceSize);
            }
        }
            
        public void SimulatePointer(PointerEventType pointerEventType, Vector2 position, int id = 0)
        {
            PointerState.PointerInputEvents.Add(new PointerDeviceState.InputEvent { Id = id, Position = position, Type = pointerEventType });
        }

        public override void LockPosition(bool forceCenter = false)
        {
            positionLocked = true;
            capturedPosition = forceCenter ? new Vector2(0.5f) : Position;
        }

        public override void UnlockPosition()
        {
            positionLocked = false;
        }
    }
}