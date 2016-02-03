// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_XENKO_UI_SDL
using System;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Graphics.SDL
{
    using SDL2;

    /// <summary>
    /// Basic operations to show, hide and load a cursor.
    /// </summary>
    public class Cursor
    {
#region Initialization
        /// <summary>
        ///  Initialize a cursor with <paramref name="data"/> and <paramref name="mask"/>. For more details, see 
        /// https://wiki.libsdl.org/SDL_CreateCursor.
        /// </summary>
        /// <param name="data">Actual cursor image</param>
        /// <param name="mask">Mask for <paramref name="data"/></param>
        /// <param name="w">Width of cursor</param>
        /// <param name="h">Height of cursor</param>
        /// <param name="hot_x">Hotspot X coordinate of cursor</param>
        /// <param name="hot_y">Hotspot Y coordinate of cursor</param>
        public Cursor(byte [] data, byte [] mask, int w, int h, int hot_x, int hot_y)
        {
            Handle = SDL.SDL_CreateCursor(data, mask, w, h, hot_x, hot_y);
        }
#endregion

#region Access
        /// <summary>
        /// Access to low level pointer to the SDL_Cursor struct.
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// Position of cursor on screen.
        /// </summary>
        public static Point Position {
            get { return Application.MousePosition; }
            set { Application.MousePosition = value; }
        }
#endregion

#region Basic Operations
        /// <summary>
        /// Hide cursor.
        /// </summary>
        public static void Hide()
        {
            SDL.SDL_ShowCursor(SDL2.SDL.SDL_DISABLE);
        }

        /// <summary>
        /// Show cursor.
        /// </summary>
        public static void Show()
        {
            SDL.SDL_ShowCursor(SDL2.SDL.SDL_ENABLE);
        }

        /// <summary>
        /// Set cursor with <paramref name="cur"/>.
        /// </summary>
        /// <param name="cur">New cursor to show.</param>
        public static void SetCursor(Cursor cur)
        {
            SDL.SDL_SetCursor(cur.Handle);
        }
#endregion
    }
}
#endif
