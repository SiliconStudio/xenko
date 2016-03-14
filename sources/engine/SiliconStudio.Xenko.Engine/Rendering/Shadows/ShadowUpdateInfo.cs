// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.Shadows
{
    public class ShadowUpdateInfo
    {
        public int CascadeCount;

        // Parameter keys
        public ParameterKey ShadowMapReceiverInfoKey;

        public ParameterKey ShadowMapReceiverVsmInfoKey;

        public ParameterKey ShadowMapLevelReceiverInfoKey;

        public ValueParameterKey<int> ShadowMapLightCountKey;

        public ObjectParameterKey<Texture> ShadowMapTextureKey;
    };
}