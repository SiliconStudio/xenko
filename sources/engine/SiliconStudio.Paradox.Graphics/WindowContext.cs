// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Graphics
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
        /// <param name="nativeHandle">The native handle.</param>
        public WindowHandle(AppContextType context, object nativeHandle)
        {
            Context = context;
            NativeHandle = nativeHandle;
        }

        /// <summary>
        /// The context.
        /// </summary>
        public readonly AppContextType Context;

        /// <summary>
        /// The native handle.
        /// </summary>
        public readonly object NativeHandle;
    }
}