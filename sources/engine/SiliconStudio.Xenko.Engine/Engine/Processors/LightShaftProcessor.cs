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
                data.SampleCount = pair.Key.SampleCount;
                data.SeparateBoundingVolumes = pair.Key.SeparateBoundingVolumes;
            }
        }

        public class Data
        {
            public LightShaftComponent Component;
            public Matrix LightWorld;
            public IDirectLight Light;
            public LightComponent LightComponent;
            public float ExtinctionFactor;
            public float ExtinctionRatio;
            public float DensityFactor;
            public bool SeparateBoundingVolumes;
            public int SampleCount;
            public readonly List<LightShaftBoundingVolumeComponent> BoundingVolumes = new List<LightShaftBoundingVolumeComponent>();
        }
    }
}