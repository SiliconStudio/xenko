// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Rendering.Lights
{
    public static partial class DirectLightGroupKeys
    {
        public static ParameterKey<T> GetParameterKey<T>(ParameterKey<T> key, int lightGroupIndex)
        {
            if (key == null) throw new ArgumentNullException("key");
            return key.ComposeIndexer("directLightGroups", lightGroupIndex);
        }
    }
}
