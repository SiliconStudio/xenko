// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Engine
{
    public class LightShaftData
    {
        public Matrix LightWorld;
        public IDirectLight Light;
        public LightComponent LightComponent;
        public LightShadowMapTexture ShadowMapTexture;
        public float ExtinctionFactor;
        public float ExtinctionRatio;
        public float DensityFactor;
    }

    public class LightShaftProcessor : EntityProcessor<LightShaftComponent, LightShaftData>
    {
        private PoolListStruct<LightShaftData> activeLightShafts = new PoolListStruct<LightShaftData>(4, () => new LightShaftData());

        public IEnumerable<LightShaftData> LightShafts => activeLightShafts;

        protected override LightShaftData GenerateComponentData(Entity entity, LightShaftComponent component)
        {
            return new LightShaftData();
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
                var data = activeLightShafts.Add();
                data.LightComponent = light;
                data.Light = light?.Type as IDirectLight;
                data.LightWorld = entity.Transform.WorldMatrix;
                data.ExtinctionFactor = pair.Key.ExtinctionFactor;
                data.ExtinctionRatio = pair.Key.ExtinctionRatio;
                data.DensityFactor = pair.Key.DensityFactor;
            }
        }
    }

    [Display("Light Shaft")]
    [DataContract("LightShaftComponent")]
    [DefaultEntityComponentProcessor(typeof(LightShaftProcessor), ExecutionMode = ExecutionMode.All)]
    public class LightShaftComponent : EntityComponent
    {
        [DataMember(-10)] public bool Enabled = true;

        [DataMember(0)] public float ExtinctionFactor = 0.001f;

        [DataMember(10)] public float ExtinctionRatio = 0.9f;

        [DataMember(20)] public float DensityFactor = 0.01f;
    }
}