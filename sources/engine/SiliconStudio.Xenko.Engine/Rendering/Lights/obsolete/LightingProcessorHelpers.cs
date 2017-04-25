// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Xenko.Effects.Renderers;
using SiliconStudio.Xenko.Effects.Shadows;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Effects.Processors
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
