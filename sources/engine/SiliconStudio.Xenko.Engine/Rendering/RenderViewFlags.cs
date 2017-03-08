using System;

namespace SiliconStudio.Xenko.Rendering
{
    [Flags]
    public enum RenderViewFlags
    {
        /// <summary>
        /// Nothing special.
        /// </summary>
        None = 0,

        /// <summary>
        /// Not being drawn directly (i.e. shared view for VR eyes).
        /// </summary>
        NotDrawn = 1,
    }
}