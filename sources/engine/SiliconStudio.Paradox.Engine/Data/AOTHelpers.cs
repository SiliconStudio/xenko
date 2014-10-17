// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Data;

namespace SiliconStudio.Paradox.Data
{
    internal static class AOTHelpers
    {
        public static void AOTHelper()
        {
            typeof(ListDataConverter<VertexBufferBindingData[], VertexBufferBinding[], VertexBufferBindingData, VertexBufferBinding>).ToString();
            typeof(ListDataConverter<LightingConfigurationData[], LightingConfiguration[], LightingConfigurationData, LightingConfiguration>).ToString();
            typeof(ListDataConverter<List<ShadowConfigurationData>, List<ShadowConfiguration>, ShadowConfigurationData, ShadowConfiguration>).ToString();
            typeof(ListDataConverter<FastCollection<EntityComponentReference<TransformationComponent>>, FastCollection<TransformationComponent>, EntityComponentReference<TransformationComponent>, TransformationComponent>).ToString();
        }
    }
}