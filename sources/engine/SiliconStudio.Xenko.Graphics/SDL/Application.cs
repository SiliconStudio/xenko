// Copyright (c) 2015 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_UI_SDL2
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Extensions;

namespace SiliconStudio.Xenko.Graphics.SDL
{
        // Using is here otherwise it would conflict with the current namespace that also defines SDL.
    using SDL2;

    public static class Application
    {
        /// <summary>
        /// Initialize Application for handling events and available windows.
        /// </summary>
        static Application()
        {
            InternalWindows = new Dictionary<IntPtr, WeakReference<Control>>(10);
        }

        /// <summary>
        /// Register <paramref name="c"/> to the list of available windows.
        /// </summary>
        /// <param name="c">Control to register</param>
        public static void RegisterControl(Control c)
        {
            lock (InternalWindows)
            {
                InternalWindows.Add(c.SdlHandle, new WeakReference<Control>(c));
            }
        }

        /// <summary>
        /// Unregister <paramref name="c"/> from the list of available windows.
        /// </summary>
        /// <param name="c">Control to unregister</param> 
        public static void UnregisterControl(Control c)
        {
            lock (InternalWindows)
            {
                InternalWindows.Remove(c.SdlHandle);
            }
        }

        /// <summary>
        /// Window that currently has the focus.
        /// </summary>
        public static Control WindowWithFocus { get; private set; }

        /// <summary>
        /// Screen coordinate of the mouse.
        /// </summary>
        public static Point MousePosition
        {
            get
            {
                Control focusedWindow = WindowWithFocus;
                if (focusedWindow != null)
                {
                    int x, y;
                        // Get the coordinate of the mouse in the focused window
                    SDL.SDL_GetMouseState(out x, out y);
                    Point windowPos = focusedWindow.Location;
                        // Use the focused window coordinate to compute the screen coordinate of the mouse.
                    return new Point(windowPos.X + x, windowPos.Y + y);
                }
                else
                {
                    throw new NotSupportedException("Cannot query a mouse position without any windows!");
                }
            }
            set
            {
                Control focusedWindow = WindowWithFocus;
                if (focusedWindow != null)
                {
                    Point windowPos = WindowWithFocus.Location;
                        // Use the focused window coordinate to compute the local coordinate of the mouse.
                    SDL.SDL_WarpMouseInWindow(WindowWithFocus.SdlHandle, value.X - windowPos.X, value.Y - windowPos.Y);
                }
                else
                {
                    throw new NotSupportedException("Cannot set mouse position without any windows!");
                }
            }
        }

        /// <summary>
        /// List of windows managed by the application.
        /// </summary>
        public static List<Control> Windows
        {
            get
            {
                lock (InternalWindows)
                {
                    var res = new List<Control>(InternalWindows.Count);
                    List<IntPtr> toRemove = null;
                    foreach (var weakRef in InternalWindows)
                    {
                        Control ctrl;
                        if (weakRef.Value.TryGetTarget(out ctrl))
                        {
                            res.Add(ctrl);
                        }
                        else
                        {
                                // Control was reclaimed without being unregistered first.
                                // We add it to `toRemove' to remove it from InternalWindows later.
                            if (toRemove == null)
                            {
                                toRemove = new List<IntPtr>(5);
                            }
                            toRemove.Add(weakRef.Key);
                        }
                    }
                        // Clean InternalWindows from controls that have been collected.
                    if (toRemove != null)
                    {
                        foreach (var w in toRemove)
                        {
                            InternalWindows.Remove(w);
                        }
                    }
                    return res;
                }
            }
        }

        /// <summary>
        /// Process a single event and dispatch it to the right window.
        /// </summary>
        public static void ProcessEvent(SDL.SDL_Event e)
        {
            Control ctrl = null;

                // Code below is to extract the associated `Control' instance and to find out the window
                // with focus. In the future, we could even add events handled at the application level.
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    ctrl = WindowFromSdlHandle(SDL.SDL_GetWindowFromID(e.button.windowID));
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    ctrl = WindowFromSdlHandle(SDL.SDL_GetWindowFromID(e.motion.windowID));
                    break;

                case SDL.SDL_EventType.SDL_MOUSEWHEEL:
                    ctrl = WindowFromSdlHandle(SDL.SDL_GetWindowFromID(e.wheel.windowID));
                    break;
                    
                case SDL.SDL_EventType.SDL_KEYDOWN:
                case SDL.SDL_EventType.SDL_KEYUP:
                    ctrl = WindowFromSdlHandle(SDL.SDL_GetWindowFromID(e.key.windowID));
                    break;

                case SDL.SDL_EventType.SDL_TEXTEDITING:
                    ctrl = WindowFromSdlHandle(SDL.SDL_GetWindowFromID(e.edit.windowID));
                    break;

                case SDL.SDL_EventType.SDL_TEXTINPUT:
                    ctrl = WindowFromSdlHandle(SDL.SDL_GetWindowFromID(e.text.windowID));
                    break;

                case SDL.SDL_EventType.SDL_WINDOWEVENT:
                {
                    ctrl = WindowFromSdlHandle(SDL.SDL_GetWindowFromID(e.window.windowID));
                    switch (e.window.windowEvent)
                    {
                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                            WindowWithFocus = ctrl;
                            break;

                        case SDL.SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                            WindowWithFocus = null;
                            break;
                    }
                    break;
                }
            }
            ctrl?.ProcessEvent(e);
        }

        /// <summary>
        /// Given a SDL Handle of a SDL window, retrieve the corresponding managed object. If object
        /// was already garbage collected, we will also clean up <see cref="InternalWindows"/>.
        /// </summary>
        /// <param name="w">SDL Handle of the window we are looking for</param>
        /// <returns></returns>
        private static Control WindowFromSdlHandle(IntPtr w)
        {
            lock (InternalWindows)
            {
                WeakReference<Control> weakRef;
                if (InternalWindows.TryGetValue(w, out weakRef))
                {
                    Control ctrl;
                    if (weakRef.TryGetTarget(out ctrl))
                    {
                        return ctrl;
                    } 
                    else
                    {
                            // Control does not exist anymore in our code. Clean `controls'.
                        InternalWindows.Remove(w);
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Backup storage for windows of current application.
        /// </summary>
        private readonly static Dictionary<IntPtr, WeakReference<Control>> InternalWindows;
    }
}
#endif
