// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering.Lights;

namespace SiliconStudio.Xenko.Engine.Processors
{
    public class LightShaftProcessor : EntityProcessor<LightShaftComponent, LightShaftProcessor.Data>
    {
        private PoolListStruct<Data> activeLightShafts = new PoolListStruct<Data>(4, () => new Data());

        public PoolListStruct<Data> LightShafts => activeLightShafts;

        protected override Data GenerateComponentData(Entity entity, LightShaftComponent component)
        {
            return new Data();
        }

        public override void Update(GameTime time)
        {
            base.Update(time);

            activeLightShafts.Clear();
            foreach (var pair in ComponentDatas)
            {
                if (!pair.Key.Enabled)
                    continue;

                var entity = pair.Key.Entity;
                var light = entity.Get<LightComponent>();
                if (light == null)
                    continue;

                var directLight = light.Type as IDirectLight;
                if (directLight == null)
                    continue;

                var data = activeLightShafts.Add();
                data.Component = pair.Key;
                data.LightComponent = light;
                data.Light = directLight;
            }
        }

        public class Data
        {
            public LightShaftComponent Component;
            public LightComponent LightComponent;
            public IDirectLight Light;
            public int SampleCount => Component.SampleCount;
            public Matrix LightWorld => Component.Entity.Transform.WorldMatrix;
            public float ExtinctionFactor => Component.ExtinctionFactor;
            public float DensityFactor => Component.DensityFactor;
            public float ExtinctionRatio => Component.ExtinctionRatio;
        }
    }
}