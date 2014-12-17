// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Paradox.Effects.Modules.Renderers;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Processors
{
    public class LightingProcessorHelpers
    {
        public static ShadowUpdateInfo CreateShadowUpdateInfo(int index, int level)
        {
            var info = new ShadowUpdateInfo { CascadeCount = level };

            // Prepare keys for this shadow map type
            var shadowSubKey = string.Format(".shadows[{0}]", index);
            info.ShadowMapReceiverInfoKey = ParameterKeys.AppendKey(ShadowMapRenderer.Receivers, shadowSubKey);
            info.ShadowMapReceiverVsmInfoKey = ParameterKeys.AppendKey(ShadowMapRenderer.ReceiversVsm, shadowSubKey);
            info.ShadowMapLevelReceiverInfoKey = ParameterKeys.AppendKey(ShadowMapRenderer.LevelReceivers, shadowSubKey);
            info.ShadowMapLightCountKey = ParameterKeys.AppendKey(ShadowMapRenderer.ShadowMapLightCount, shadowSubKey);
            info.ShadowMapTextureKey = ParameterKeys.AppendKey(ShadowMapKeys.Texture, shadowSubKey);

            return info;
        }
    }
}
