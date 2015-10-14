// Copyright (c) 2011 Silicon Studio

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// Level defined for a cascade shadow map.
    /// </summary>
    public enum CascadeShadowMapLevel
    {
        /// <summary>
        /// Use only one view.
        /// </summary>
        X1 = 1,

        /// <summary>
        /// Use two views.
        /// </summary>
        X2 = 2,

        /// <summary>
        /// Use four views.
        /// </summary>
        X4 = 4
    }
}