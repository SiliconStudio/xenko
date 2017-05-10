// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_XENKO_UI_SDL
using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A known gamepad that uses SDL as a driver
    /// </summary>
    internal class GamePadSDL : GamePadFromLayout, IGamePadIndexAssignable, IDisposable
    {
        private bool disposed;

        public GamePadSDL(InputSourceSDL source, InputManager inputManager, GameControllerSDL controller, GamePadLayout layout)
            : base(inputManager, controller, layout)
        {
            Source = source;
            Name = controller.Name;
            Id = controller.Id;
            ProductId = controller.ProductId;
        }

        public void Dispose()
        {
            if (!disposed)
            {
                // ReSharper disable once PossibleNullReferenceException (checked in constructor)
                (GameControllerDevice as GameControllerSDL).Dispose();

                disposed = true;
            }
        }

        public override string Name { get; }

        public override Guid Id { get; }

        public override Guid ProductId { get; }

        public override IInputSource Source { get; }

        public new int Index
        {
            get { return base.Index; }
            set { SetIndexInternal(value, false); }
        }

        public override void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight)
        {
            // No vibration support in SDL gamepads
        }
    }
}
#endif