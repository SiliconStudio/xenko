// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP && SILICONSTUDIO_XENKO_UI_SDL
using System;
using SiliconStudio.Xenko.Graphics.SDL;
using SiliconStudio.Core.Mathematics;
using SDL2;

namespace SiliconStudio.Xenko.Games
{

    /// <summary>
    /// Default Rendering Form on SDL based applications.
    /// </summary>
    public class GameFormSdl : Window
    {
#region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="GameForm"/> class.
        /// </summary>
        public GameFormSdl() : this("Xenko Game")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameForm"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public GameFormSdl(String text) : base(text)
        {
            Size = new Size2(800, 600);
            ResizeBeginActions += GameForm_ResizeBeginActions;
            ResizeEndActions += GameForm_ResizeEndActions;
            ActivateActions += GameForm_ActivateActions;
            DeActivateActions += GameForm_DeActivateActions;
            previousWindowState = FormWindowState.Normal;
            MinimizedActions += GameForm_MinimizedActions;
            MaximizedActions += GameForm_MaximizedActions;
            RestoredActions += GameForm_RestoredActions;
        }
#endregion

#region Events
        /// <summary>
        /// Occurs when [app activated].
        /// </summary>
        public event EventHandler<EventArgs> AppActivated;

        /// <summary>
        /// Occurs when [app deactivated].
        /// </summary>
        public event EventHandler<EventArgs> AppDeactivated;

        /// <summary>
        /// Occurs when [pause rendering].
        /// </summary>
        public event EventHandler<EventArgs> PauseRendering;

        /// <summary>
        /// Occurs when [resume rendering].
        /// </summary>
        public event EventHandler<EventArgs> ResumeRendering;

        /// <summary>
        /// Occurs when [user resized].
        /// </summary>
        public event EventHandler<EventArgs> UserResized;
#endregion

#region Implementation
//
// TODO: The code below is taken from GameForm.cs of the Windows Desktop implementation. This needs reviewing
//
        private Size2 cachedSize;
        private FormWindowState previousWindowState;
        //private DisplayMonitor monitor;
        private bool isUserResizing;
        private bool isSizeChangedWithoutResizeBegin;
        private bool isActive;

        private void GameForm_MinimizedActions(SDL.SDL_WindowEvent e)
        {
            previousWindowState = FormWindowState.Minimized;
            PauseRendering?.Invoke(this, EventArgs.Empty);
        }

        private void GameForm_MaximizedActions(SDL.SDL_WindowEvent e)
        {
            if (previousWindowState == FormWindowState.Minimized)
                ResumeRendering?.Invoke(this, EventArgs.Empty);

            previousWindowState = FormWindowState.Maximized;

            UserResized?.Invoke(this, EventArgs.Empty);
            //UpdateScreen();
            cachedSize = Size;
        }

        private void GameForm_RestoredActions(SDL.SDL_WindowEvent e)
        {
            if (previousWindowState == FormWindowState.Minimized)
                ResumeRendering?.Invoke(this, EventArgs.Empty);

            var newSize = Size;

            if (!isUserResizing && (!newSize.Equals(cachedSize) || previousWindowState == FormWindowState.Maximized))
            {
                previousWindowState = FormWindowState.Normal;

                // Only update when cachedSize is != 0
                if (cachedSize != Size2.Empty)
                {
                    isSizeChangedWithoutResizeBegin = true;
                }
            }

            previousWindowState = FormWindowState.Normal;
        }

        private void GameForm_DeActivateActions(SDL.SDL_WindowEvent e)
        {
            AppActivated?.Invoke(this, EventArgs.Empty);
            isActive = false;
        }

        private void GameForm_ActivateActions(SDL.SDL_WindowEvent e)
        {
            AppDeactivated?.Invoke(this, EventArgs.Empty);
            isActive = true;
        }


        private void GameForm_ResizeBeginActions(SDL.SDL_WindowEvent e)
        {
            isUserResizing = true;
            cachedSize = Size;
            PauseRendering?.Invoke(this, EventArgs.Empty);
        }

        private void GameForm_ResizeEndActions(SDL.SDL_WindowEvent e)
        {
            if (isUserResizing && cachedSize.Equals(Size))
            {
                UserResized?.Invoke(this, EventArgs.Empty);
                // UpdateScreen();
            }

            isUserResizing = false;
            ResumeRendering?.Invoke(this, EventArgs.Empty);
        }
#endregion
    }
}
#endif
