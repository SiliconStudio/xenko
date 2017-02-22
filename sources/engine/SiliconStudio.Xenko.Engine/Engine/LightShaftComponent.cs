// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Collections;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Lights;
using SiliconStudio.Xenko.Rendering.Shadows;

namespace SiliconStudio.Xenko.Engine
{
    public class LightShaftData
    {
        public LightShaftComponent Component;
        public Matrix LightWorld;
        public IDirectLight Light;
        public LightComponent LightComponent;
        public float ExtinctionFactor;
        public float ExtinctionRatio;
        public float DensityFactor;
        public bool SeparateBoundingVolumes;
        public readonly List<LightShaftBoundingVolumeComponent> BoundingVolumes = new List<LightShaftBoundingVolumeComponent>();
    }

    public class LightShaftProcessor : EntityProcessor<LightShaftComponent, LightShaftData>
    {
        private PoolListStruct<LightShaftData> activeLightShafts = new PoolListStruct<LightShaftData>(4, () => new LightShaftData());

        public PoolListStruct<LightShaftData> LightShafts => activeLightShafts;
        
        protected override LightShaftData GenerateComponentData(Entity entity, LightShaftComponent component)
        {
            return new LightShaftData();
        }

        public override void Update(GameTime time)
        {
            base.Update(time);

            // TODO: prevent doing this every time
            // Link bounding volumes to light shafts
            foreach (var v in ComponentDatas.Values)
            {
                v.BoundingVolumes.Clear();
            }

            activeLightShafts.Clear();
            foreach (var pair in ComponentDatas)
            {
                if (!pair.Key.Enabled)
                    continue;

                var entity = pair.Key.Entity;
                var light = entity.Get<LightComponent>();
                var data = activeLightShafts.Add();
                data.Component = pair.Key;
                data.LightComponent = light;
                data.Light = light?.Type as IDirectLight;
                data.LightWorld = entity.Transform.WorldMatrix;
                data.ExtinctionFactor = pair.Key.ExtinctionFactor;
                data.ExtinctionRatio = pair.Key.ExtinctionRatio;
                data.DensityFactor = pair.Key.DensityFactor;
                data.SeparateBoundingVolumes = pair.Key.SeparateBoundingVolumes;
            }
        }
    }

    [Display("Light Shaft")]
    [DataContract("LightShaftComponent")]
    [DefaultEntityComponentProcessor(typeof(LightShaftProcessor), ExecutionMode = ExecutionMode.All)]
    public class LightShaftComponent : ActivableEntityComponent
    {
        public float ExtinctionFactor { get; set; } = 0.001f;

        public float ExtinctionRatio { get; set; } = 0.9f;

        public float DensityFactor { get; set; } = 0.01f;

        /// <summary>
        /// If true, all bounding volumes will be drawn one by one. If not, they will be combined (but lower quality if they overlap)
        /// </summary>
        public bool SeparateBoundingVolumes { get; set; } = true;
    }
}