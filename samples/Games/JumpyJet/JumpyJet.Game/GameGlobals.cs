// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Engine.Events;

namespace JumpyJet
{
    static class GameGlobals
    {
        public static EventKey GameOverEventKey = new EventKey("Global", "Game Over");
        public static EventKey GameStartedventKey = new EventKey("Global", "Game Reset");
        public static EventKey GameResetEventKey = new EventKey("Global", "Game Started");
        public static EventKey PipePassedEventKey = new EventKey("Global", "Pipe Passed");

        public const float GameSpeed = 2.90f;
        public const float GamePixelToUnitScale = 0.01f;
    }
}
