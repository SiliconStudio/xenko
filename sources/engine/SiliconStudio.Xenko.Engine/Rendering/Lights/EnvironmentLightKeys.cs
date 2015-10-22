// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    public static class EnvironmentLightKeys
    {
        public static ParameterKey<T> GetParameterKey<T>(ParameterKey<T> key, int lightIndex)
        {
            if (key == null) throw new ArgumentNullException("key");
            return key.ComposeIndexer("environmentLights", lightIndex);
        }
    }
}