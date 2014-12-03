// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects.Renderers;
using SiliconStudio.Paradox.Effects.ShadowMaps;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Processors
{
    public class LightingProcessorHelpers
    {
        public static ShadowUpdateInfo CreateShadowUpdateInfo(int index, int level)
        {
            var info = new ShadowUpdateInfo { CascadeCount = level };

            // Prepare keys for this shadow map type
            // TODO: use StringBuilder instead
            var shadowSubKey = string.Format("shadows[{0}]", index);
            info.ShadowMapReceiverInfoKey = ShadowMapRenderer.Receivers.ComposeWith(shadowSubKey);
            info.ShadowMapReceiverVsmInfoKey = ShadowMapRenderer.ReceiversVsm.ComposeWith(shadowSubKey);
            info.ShadowMapLevelReceiverInfoKey = ShadowMapRenderer.LevelReceivers.ComposeWith(shadowSubKey);
            info.ShadowMapLightCountKey = ShadowMapRenderer.ShadowMapLightCount.ComposeWith(shadowSubKey);
            info.ShadowMapTextureKey = ShadowMapKeys.Texture.ComposeWith(shadowSubKey);

            return info;
        }
    }
}
