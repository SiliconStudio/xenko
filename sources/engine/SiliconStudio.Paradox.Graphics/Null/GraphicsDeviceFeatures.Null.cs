// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#if SILICONSTUDIO_PARADOX_GRAPHICS_API_NULL 

namespace SiliconStudio.Paradox.Graphics
{
    public partial struct GraphicsDeviceFeatures
    {
        internal GraphicsDeviceFeatures(GraphicsDevice device)
        {
            IsProfiled = false;
            HasDriverCommandLists = false;
            HasMultiThreadingConcurrentResources = false;
            HasComputeShaders = false;
            HasDoublePrecision = false;
            mapFeaturesPerFormat = new FeaturesPerFormat[0];
            Profile = GraphicsProfile.Level9;
        }
    }
}
#endif