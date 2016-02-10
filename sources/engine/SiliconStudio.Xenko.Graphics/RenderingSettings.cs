// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Data;

namespace SiliconStudio.Xenko.Graphics
{
    //Workaround needed for now, since we don't support orientation changes during game play
    public enum RequiredDisplayOrientation
    {
        /// <summary>
        /// The default value for the orientation.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Displays in landscape mode to the left.
        /// </summary>
        [Display("Landscape Left")]
        LandscapeLeft = 1,

        /// <summary>
        /// Displays in landscape mode to the right.
        /// </summary>
        [Display("Landscape Right")]
        LandscapeRight = 2,

        /// <summary>
        /// Displays in portrait mode.
        /// </summary>
        Portrait = 4
    }

    [DataContract]
    [Display("Rendering Settings")]
    public class RenderingSettings : Configuration
    {
        /// <summary>
        /// Gets or sets the width of the back buffer.
        /// </summary>
        /// <userdoc>
        /// The desired back buffer width.
        /// Might be overriden depending on actual device resolution and/or ratio.
        /// On Windows, it will be the window size. On Android/iOS, it will be the off-screen target resolution.
        /// </userdoc>
        [DataMember(0)]
        public int DefaultBackBufferWidth = 1280;

        /// <summary>
        /// Gets or sets the height of the back buffer.
        /// </summary>
        /// <userdoc>
        /// The desired back buffer height.
        /// Might be overriden depending on actual device resolution and/or ratio.
        /// On Windows, it will be the window size. On Android/iOS, it will be the off-screen target resolution.
        /// </userdoc>
        [DataMember(10)]
        public int DefaultBackBufferHeight = 720;

        /// <summary>
        /// Gets or sets the default graphics profile.
        /// </summary>
        /// <userdoc>The graphics feature level this game require.</userdoc>
        [DataMember(20)]
        public GraphicsProfile DefaultGraphicsProfile = GraphicsProfile.Level_10_0;

        /// <summary>
        /// Gets or sets the colorspace.
        /// </summary>
        /// <value>The colorspace.</value>
        /// <userdoc>The colorspace (Gamma or Linear) used for rendering. This value affects both the runtime and editor.</userdoc>
        [DataMember(30)]
        public ColorSpace ColorSpace = ColorSpace.Linear;

        /// <summary>
        /// Gets or sets the display orientation.
        /// </summary>
        /// <userdoc>The display orientations this game support.</userdoc>
        [DataMember(40)]
        public RequiredDisplayOrientation DisplayOrientation = RequiredDisplayOrientation.Default;
    }
}