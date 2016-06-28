// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// A platform specific window handle.
    /// </summary>
    public class WindowHandle
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowHandle"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="nativeWindow">The native window instance (Winforms, SDLWindow, ...).</param>
        /// <param name="handle">The associated handle of <paramref name="nativeWindow"/>.</param>
        public WindowHandle(AppContextType context, object nativeWindow, IntPtr handle)
        {
            Context = context;
            NativeWindow = nativeWindow;
            Handle = handle;
        }

        /// <summary>
        /// The context.
        /// </summary>
        public readonly AppContextType Context;

        /// <summary>
        /// The native windows as an opaque <see cref="System.Object"/>.
        /// </summary>
        public object NativeWindow { get; }

        /// <summary>
        /// The associated platform specific handle of <seealso cref="NativeWindow"/>.
        /// </summary>
        public IntPtr Handle { get; }
    }
}
