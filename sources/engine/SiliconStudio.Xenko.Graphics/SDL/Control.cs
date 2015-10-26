// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_UI_SDL2
using System;

namespace SiliconStudio.Xenko.Graphics.SDL
{
        // Using is here otherwise it would conflict with the current namespace that also defines SDL.
    using SDL2;

    public class Control: IDisposable
    {
#region Initialization
        /// <summary>
        /// Type initializer for `Control' which automatically initializes the SDL infrastructure.
        /// </summary>
        static Control()
        {
            SDL.SDL_Init(SDL.SDL_INIT_EVERYTHING);
                // Disable effect of doing Alt+F4
            SDL.SDL_SetHint(SDL.SDL_HINT_WINDOWS_NO_CLOSE_ON_ALT_F4, "1");
        }

        /// <summary>
        /// Initialize current instance with <paramref name="title"/> as the title of the Control.
        /// </summary>
        /// <param name="title">Title of the window, see Text property.</param>
        public Control(string title)
        {
                // Create the SDL window and then extract the native handle.
            SdlHandle = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, 640, 480,
                SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE | SDL.SDL_WindowFlags.SDL_WINDOW_HIDDEN);

            if (SdlHandle == IntPtr.Zero)
            {
                throw new Exception("Cannot allocate SDL Window: " + SDL.SDL_GetError()); 
            }
            else
            {
                SDL.SDL_SysWMinfo info = default(SDL.SDL_SysWMinfo);
                SDL.SDL_VERSION(out info.version);
                SDL.SDL_bool res = SDL.SDL_GetWindowWMInfo(SdlHandle, ref info);
                if (res == SDL.SDL_bool.SDL_FALSE)
                {
                    throw new Exception("Cannot get Window information: " + SDL.SDL_GetError());
                }
                else
                {
#if SILICONSTUDIO_PLATFORM_WINDOWS_DESKTOP
                    Handle = info.info.win.window;
#endif
                }
            }
            Application.RegisterControl(this);
        }
#endregion


        /// <summary>
        /// Move window to Back.
        /// FIXME: This is not yet implemented on SDL.
        /// </summary>
        public virtual void SendToBack()
        {
            Point loc = Location;
            SDL.SDL_SetHint(SDL.SDL_HINT_ALLOW_TOPMOST, "0");
            SDL.SDL_SetWindowPosition(SdlHandle, loc.X, loc.Y);
        }

        public virtual void BringToFront()
        {
            Point loc = Location;
            SDL.SDL_SetHint(SDL.SDL_HINT_ALLOW_TOPMOST, "1");
            SDL.SDL_SetWindowPosition(SdlHandle, loc.X, loc.Y);
        }

        /// <summary>
        /// Get or set the mouse position on screen.
        /// </summary>
        public Point MousePosition
        {
            get { return Application.MousePosition; }
            set { Application.MousePosition = value; }
        }

        public bool TopMost
        {
            get { return SDL.SDL_GetHint(SDL.SDL_HINT_ALLOW_TOPMOST) == "1"; }
            set { SDL.SDL_SetHint(SDL.SDL_HINT_ALLOW_TOPMOST, (value ? "1" : "0")); }
        }

        public void Show()
        {
            if (!_hasBeenShownOnce)
            {
                _hasBeenShownOnce = true;
                HandleCreated?.Invoke(this, EventArgs.Empty);
            }
            SDL.SDL_ShowWindow(SdlHandle);
        }

        public bool IsFullScreen {
            get
            {
                return (SDL.SDL_GetWindowFlags(SdlHandle) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN) != 0;
            }
            set
            {
                SDL.SDL_SetWindowFullscreen(SdlHandle, (uint) (value ? SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN : 0));
            }
        }

        public bool Visible {
            get
            {
                return (SDL.SDL_GetWindowFlags(SdlHandle) & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN) != 0;
            }
            set
            {
                if (value)
                {
                    SDL.SDL_ShowWindow(SdlHandle);
                }
                else
                {
                    SDL.SDL_HideWindow(SdlHandle);
                }
            }
        }

        public FormWindowState WindowState
        {
            get
            {
                uint flags = SDL.SDL_GetWindowFlags(SdlHandle);
                if ((flags & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_MAXIMIZED) != 0)
                {
                    return FormWindowState.Maximized;
                }
                else if ((flags & (uint) SDL.SDL_WindowFlags.SDL_WINDOW_MINIMIZED) != 0)
                {
                    return FormWindowState.Minimized;
                }
                else
                {
                    return FormWindowState.Normal;
                }
            }
            set
            {
                switch (value)
                {
                    case FormWindowState.Maximized:
                        SDL.SDL_MaximizeWindow(SdlHandle);
                        break;
                    case FormWindowState.Minimized:
                        SDL.SDL_MinimizeWindow(SdlHandle);
                        break;
                    case FormWindowState.Normal:
                        SDL.SDL_RestoreWindow(SdlHandle);
                        break;
                }
            }
        }

        public bool MaximizeBox { get; set; }

        public Size Size
        {
            get
            {
                int w, h;
                SDL.SDL_GetWindowSize(SdlHandle, out w, out h);
                return new Size(w, h);
            }
            set { SDL.SDL_SetWindowSize(SdlHandle, value.Width, value.Height); }
        }

        public unsafe Size ClientSize
        {
            get
            {
                SDL.SDL_Surface *surfPtr = (SDL.SDL_Surface *) SDL.SDL_GetWindowSurface(SdlHandle);
                return new Size(surfPtr->w, surfPtr->h);
            }
            set
            {
                // FIXME: We need to adapt the ClientSize to an actual Size to take into account borders.
                // FIXME: On Windows you do this by using AdjustWindowRect.
                SDL.SDL_SetWindowSize(SdlHandle, value.Width, value.Height);
            }
        }
        public unsafe Rect ClientRectangle
        {
            get
            {
                SDL.SDL_Surface *surfPtr = (SDL.SDL_Surface *) SDL.SDL_GetWindowSurface(SdlHandle);
                return new Rect(0, 0, surfPtr->w, surfPtr->h);
            }
            set
            {
                // FIXME: We need to adapt the ClientRectangle to an actual Size to take into account borders.
                // FIXME: On Windows you do this by using AdjustWindowRect.
                SDL.SDL_SetWindowSize(SdlHandle, value.Width, value.Height);
                SDL.SDL_SetWindowPosition(SdlHandle, value.X, value.Y);
            }
        }

        public Point Location
        {
            get
            {
                int x, y;
                SDL.SDL_GetWindowPosition(SdlHandle, out x, out y);
                return new Point(x, y);
            }
            set
            {
                SDL.SDL_SetWindowPosition(SdlHandle, value.X, value.Y);
            }
        }

        public string Text {
            get { return SDL.SDL_GetWindowTitle(SdlHandle); }
            set { SDL.SDL_SetWindowTitle(SdlHandle, value); }
        }

        public FormBorderStyle FormBorderStyle
        {
            get
            {
                uint flags = SDL.SDL_GetWindowFlags(SdlHandle);
                if ((flags & (uint)SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE) != 0)
                {
                    return FormBorderStyle.Sizable;
                }
                else
                {
                    return FormBorderStyle.FixedSingle;
                }
            }
            set
            {
                // FIXME: How to implement this since this is being called.
                // throw new NotImplementedException("Cannot change the border style after creation");
            }
        }

        public delegate void MouseButtonDelegate(SDL.SDL_MouseButtonEvent e);
        public delegate void MouseMoveDelegate(SDL.SDL_MouseMotionEvent e);
        public delegate void MouseWheelDelegate(SDL.SDL_MouseWheelEvent e);
        //public delegate void ExposeDelegate(Dc a_dc, Rect a_rect);
        public delegate void TextEditingDelegate(SDL.SDL_TextEditingEvent e);
        public delegate void TextInputDelegate(SDL.SDL_TextInputEvent e);
        public delegate void WindowEventDelegate(SDL.SDL_WindowEvent e);
        public delegate void KeyDelegate(SDL.SDL_KeyboardEvent e);
        public delegate void NotificationDelegate();

        public event MouseButtonDelegate PointerButtonPressActions;
        public event MouseButtonDelegate PointerButtonReleaseActions;
        public event MouseWheelDelegate MouseWheelActions;
        public event MouseMoveDelegate MouseMoveActions;
        public event KeyDelegate KeyDownActions;
        public event KeyDelegate KeyUpActions;
        public event TextEditingDelegate TextEditingActions;
        public event TextInputDelegate TextInputActions;
        public event NotificationDelegate CloseActions;
        public event WindowEventDelegate ResizeBeginActions;
        public event WindowEventDelegate ResizeEndActions;
        public event WindowEventDelegate ActivateActions;
        public event WindowEventDelegate DeActivateActions;
        public event WindowEventDelegate MinimizedActions;
        public event WindowEventDelegate MaximizedActions;
        public event WindowEventDelegate RestoredActions;
        public event WindowEventDelegate MouseEnterActions;
        public event WindowEventDelegate MouseLeaveActions;
        public event WindowEventDelegate FocusGainedActions;
        public event WindowEventDelegate FocusLostActions;

        /// <summary>
        /// Those event handlers are for backward compatibility with Windows forms.
        /// </summary>
        public event EventHandler MouseEnter;
        public event EventHandler MouseLeave;
        public event EventHandler Resize;
        public event EventHandler HandleCreated;
      
        /// <summary>
        /// Process events for the current window
        /// </summary>
        public virtual void ProcessEvent(SDL.SDL_Event e)
        {
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    PointerButtonPressActions?.Invoke(e.button);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    PointerButtonReleaseActions?.Invoke(e.button);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    MouseMoveActions?.Invoke(e.motion);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    MouseWheelActions?.Invoke(e.wheel);
                    break;

                case SDL.SDL_EventType.SDL_KEYDOWN:
                    KeyDownActions?.Invoke(e.key);
                    break;

                case SDL.SDL_EventType.SDL_KEYUP:
                    KeyUpActions?.Invoke(e.key);
                    break;

                case SDL.SDL_EventType.SDL_TEXTEDITING:
                    TextEditingActions?.Invoke(e.edit);
                    break;

                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    TextInputActions?.Invoke(e.text);
                    break;

                case SDL.SDL_EventType.SDL_WINDOWEVENT:
                {
                    switch (e.window.windowEvent)
                    {
                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                            ResizeBeginActions?.Invoke(e.window);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                            ResizeEndActions?.Invoke(e.window);
                            Resize?.Invoke(this, EventArgs.Empty);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                            CloseActions?.Invoke();
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
                            ActivateActions?.Invoke(e.window);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_HIDDEN:
                            DeActivateActions?.Invoke(e.window);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                            MinimizedActions?.Invoke(e.window);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
                            MaximizedActions?.Invoke(e.window);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                            RestoredActions?.Invoke(e.window);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_ENTER:
                            MouseEnter?.Invoke(this, EventArgs.Empty);
                            MouseEnterActions?.Invoke(e.window);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_LEAVE:
                            MouseLeave?.Invoke(this, EventArgs.Empty);
                            MouseLeaveActions?.Invoke(e.window);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                            FocusGainedActions?.Invoke(e.window);
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                            FocusLostActions?.Invoke(e.window);
                            break;

                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Platform specific handle for Control:
        /// - On Windows: the HWND of the window
        /// - On Unix: ...
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// The SDL window handle.
        /// </summary>
        public IntPtr SdlHandle { get; private set; }

        #region Disposal
        ~Control()
        {
            Dispose(false);
        }

        /// <summary>
        /// Have we already disposed of the current object?
        /// </summary>
        public bool IsDisposed
        {
            get { return SdlHandle == IntPtr.Zero; }
        }

        public event EventHandler Disposed;

        /// <summary>
        /// Dispose of current Control.
        /// </summary>
        /// <param name="disposing">If <c>false</c> we are being called from the Finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (SdlHandle != IntPtr.Zero)
            {
                if (disposing)
                {
                        // Dispose managed state (managed objects).
                    Disposed?.Invoke(this, EventArgs.Empty);
                    Application.UnregisterControl(this);
                }

                    // Free unmanaged resources (unmanaged objects) and override a finalizer below.
                SDL.SDL_DestroyWindow(SdlHandle);
                SdlHandle = IntPtr.Zero;
                Handle = IntPtr.Zero;
            }
        }
  
        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
                // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
                // Performance improvement to avoid being called a second time by the GC.
            GC.SuppressFinalize(this);
        }
#endregion

#region Implementation
        private bool _hasBeenShownOnce;
#endregion
    }
}
#endif
