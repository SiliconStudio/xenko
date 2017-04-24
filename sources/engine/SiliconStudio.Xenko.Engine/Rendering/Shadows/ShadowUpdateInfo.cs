// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
