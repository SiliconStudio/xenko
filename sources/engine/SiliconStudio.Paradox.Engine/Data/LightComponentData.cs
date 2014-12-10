// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects;

namespace SiliconStudio.Paradox.Engine.Data
{
    public partial class LightComponentData
    {
        public LightComponentData()
        {
            Enabled = true;
            // Default light color is white (better than black)
            Color = new Core.Mathematics.Color3(1.0f);

            Intensity = 1.0f;
            ShadowMapFilterType = ShadowMapFilterType.Nearest;
            Enabled = true;
            Deferred = true;
            Layers = RenderLayers.RenderLayerAll;
            SpotBeamAngle = 0;
            SpotFieldAngle = 0;
            ShadowMap = false;
            ShadowMapMaxSize = 512;
            ShadowMapMinSize = 512;
            ShadowMapCascadeCount = 1;
            ShadowNearDistance = 1.0f;
            ShadowFarDistance = 100000.0f;
            DecayStart = 100.0f;
            BleedingFactor = 0.0f;
            MinVariance = 0.0f;
        }
    }
}