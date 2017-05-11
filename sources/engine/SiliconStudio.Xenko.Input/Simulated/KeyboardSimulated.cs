// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Input
{
    public partial class InputSourceSimulated : InputSourceBase
    {
        public class KeyboardSimulated : KeyboardDeviceBase
        {
            public KeyboardSimulated()
            {
                Priority = -1000;
            }

            public override string Name => "Simulated Keyboard";

            public override Guid Id => new Guid(10, 10, 1, 0, 0, 0, 0, 0, 0, 0, 0);

            public override IInputSource Source => Instance;

            public void SimulateDown(Keys key)
            {
                HandleKeyDown(key);
            }

            public void SimulateUp(Keys key)
            {
                HandleKeyUp(key);
            }
        }
    }
}