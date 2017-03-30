// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

#if SILICONSTUDIO_XENKO_GRAPHICS_API_NULL

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Features supported by a <see cref="GraphicsDevice"/>.
    /// </summary>
    /// <remarks>This class gives also features for a particular format, using the operator this[Format] on this structure. </remarks>
    public partial struct GraphicsDeviceFeatures
    {
        internal GraphicsDeviceFeatures(GraphicsDevice deviceRoot)
        {
            NullHelper.ToImplement();
            mapFeaturesPerFormat = new FeaturesPerFormat[256];
            for (int i = 0; i < mapFeaturesPerFormat.Length; i++)
                mapFeaturesPerFormat[i] = new FeaturesPerFormat((PixelFormat)i, MSAALevel.None, FormatSupport.None);
            HasComputeShaders = true;
            HasDepthAsReadOnlyRT = false;
            HasDepthAsSRV = true;
            HasDoublePrecision = true;
            HasDriverCommandLists = true;
            HasMultiThreadingConcurrentResources = true;
            HasResourceRenaming = true;
            HasSRgb = true;
            RequestedProfile = GraphicsProfile.Level_11_2;
            CurrentProfile = GraphicsProfile.Level_11_2;
        }
    }
}
#endif
