// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine.Processors
{
    public class LightShaftBoundingVolumeProcessor : EntityProcessor<LightShaftBoundingVolumeComponent, LightShaftBoundingVolumeComponent>
    {
        private Dictionary<LightShaftComponent, List<Data>> volumesPerLightShaft = new Dictionary<LightShaftComponent, List<Data>>();

        public override void Update(GameTime time)
        {
            base.Update(time);
            volumesPerLightShaft.Clear();

            // TODO: Make this more efficient
            foreach (var pair in ComponentDatas)
            {
                if (!pair.Key.Enabled)
                    continue;

                var world = pair.Key.Entity.Transform.WorldMatrix;

                var lightShaft = pair.Key.LightShaft;
                if (lightShaft == null)
                    continue;

                List<Data> data;
                if (!volumesPerLightShaft.TryGetValue(lightShaft, out data))
                    volumesPerLightShaft.Add(lightShaft, data = new List<Data>());

                data.Add(new Data
                {
                    World = world,
                    Model = pair.Key.Model
                });
            }
        }

        public IReadOnlyList<Data> GetBoundingVolumesForComponent(LightShaftComponent component)
        {
            List<Data> data;
            if (!volumesPerLightShaft.TryGetValue(component, out data))
                return null;
            return data;
        }

        protected override LightShaftBoundingVolumeComponent GenerateComponentData(Entity entity, LightShaftBoundingVolumeComponent component)
        {
            return component;
        }

        public class Data
        {
            public Matrix World;
            public Model Model;
        }
    }
}