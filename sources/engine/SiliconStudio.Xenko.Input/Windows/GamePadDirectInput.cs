// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && (SILICONSTUDIO_XENKO_UI_WINFORMS || SILICONSTUDIO_XENKO_UI_WPF)
using System;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// A known gamepad that uses DirectInput as a driver
    /// </summary>
    internal class GamePadDirectInput : GamePadFromLayout, IGamePadIndexAssignable
    {
        public GamePadDirectInput(InputSourceWindowsDirectInput source, InputManager inputManager, GameControllerDirectInput controller, GamePadLayout layout)
            : base(inputManager, controller, layout)
        {
            Source = source;
            Name = controller.Name;
            Id = controller.Id;
            ProductId = controller.ProductId;
        }

        public new int Index
        {
            get { return base.Index; }
            set { SetIndexInternal(value, false); }
        }

        public override string Name { get; }

        public override Guid Id { get; }

        public override Guid ProductId { get; }

        public override IInputSource Source { get; }

        public override void SetVibration(float smallLeft, float smallRight, float largeLeft, float largeRight)
        {
            // No vibration support in directinput gamepads
        }
    }
}

#endif