// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A renderable item used by <see cref="IEntityComponentRenderer"/>
    /// </summary>
    [Obsolete]
    [StructLayout(LayoutKind.Sequential)]
    [DebuggerDisplay("{Renderer} Depth: {RealDepth}")]
    public struct RenderItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RenderItem"/> struct.
        /// </summary>
        /// <param name="renderer">The renderer.</param>
        /// <param name="drawContext">The draw context.</param>
        /// <param name="depth">The depth.</param>
        public RenderItem(IEntityComponentRenderer renderer, object drawContext, float depth)
        {
            if (renderer == null) throw new ArgumentNullException("renderer");
            if (drawContext == null) throw new ArgumentNullException("drawContext");
            Renderer = renderer;
            DrawContext = drawContext;

            unsafe
            {
                Depth = *(int*)&depth;

                // Ordering of negative float and int values is reversed, since negative ints are ordered (bitwise)
                // by increasing value, while negative floats are ordered by increasing absolute value
                if (Depth < 0)
                    Depth = (1 << 31) - Depth;
            }
        }

        public readonly int Depth;

        public readonly IEntityComponentRenderer Renderer;

        public readonly object DrawContext;

        private float RealDepth
        {
            get
            {
                unsafe
                {
                    var depth = Depth;
                    return *(float*)&depth;
                }
            }
        }
    }
}