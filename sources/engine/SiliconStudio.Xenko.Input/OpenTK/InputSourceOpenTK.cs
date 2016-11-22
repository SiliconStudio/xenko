// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if (SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP || SILICONSTUDIO_PLATFORM_UNIX) && SILICONSTUDIO_XENKO_GRAPHICS_API_OPENGL && SILICONSTUDIO_XENKO_UI_OPENTK

using SiliconStudio.Xenko.Games;
using GameWindow = OpenTK.GameWindow;

namespace SiliconStudio.Xenko.Input
{
    /// <summary>
    /// Provides support for mouse/keyboard input using OpenTK
    /// </summary>
    internal class InputSourceOpenTK : InputSourceBase
    {
        private GameWindow gameWindow;
        private GameContext<OpenTK.GameWindow> gameContext;

        private KeyboardOpenTK keyboard;
        private MouseOpenTK mouse;

        public override void Dispose()
        {
            base.Dispose();
            keyboard?.Dispose();
            mouse?.Dispose();
        }

        public override void Initialize(InputManager inputManager)
        {
            gameContext = inputManager.Game.Context as GameContext<OpenTK.GameWindow>;
            gameWindow = gameContext.Control;

            keyboard = new KeyboardOpenTK(gameWindow);
            mouse = new MouseOpenTK(inputManager.Game, gameWindow);

            RegisterDevice(keyboard);
            RegisterDevice(mouse);
        }
    }
}

#endif