// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine.Design;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Rendering;

namespace SiliconStudio.Xenko.Engine
{
    public class LightShaftBoundingVolumeData
    {
        public Matrix World;
        public Model Model;
    }

    public class LightShaftBoundingVolumeProcessor : EntityProcessor<LightShaftBoundingVolumeComponent, LightShaftBoundingVolumeComponent>
    {
        private Dictionary<LightShaftComponent, List<LightShaftBoundingVolumeData>> volumesPerLightShaft = new Dictionary<LightShaftComponent, List<LightShaftBoundingVolumeData>>();
        
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

                List<LightShaftBoundingVolumeData> data;
                if (!volumesPerLightShaft.TryGetValue(lightShaft, out data))
                    volumesPerLightShaft.Add(lightShaft, data = new List<LightShaftBoundingVolumeData>());

                data.Add(new LightShaftBoundingVolumeData
                {
                    World = world,
                    Model = pair.Key.Model
                });
            }
        }

        public IReadOnlyList<LightShaftBoundingVolumeData> GetBoundingVolumesForComponent(LightShaftComponent component)
        {
            List<LightShaftBoundingVolumeData> data;
            if (!volumesPerLightShaft.TryGetValue(component, out data))
                return null;
            return data;
        }

        protected override LightShaftBoundingVolumeComponent GenerateComponentData(Entity entity, LightShaftBoundingVolumeComponent component)
        {
            return component;
        }
    }

    [Display("Light Shaft Bounding Volume")]
    [DataContract("LightShaftBoundingVolumeComponent")]
    [DefaultEntityComponentProcessor(typeof(LightShaftBoundingVolumeProcessor))]
    public class LightShaftBoundingVolumeComponent : ActivableEntityComponent
    {
        public Model Model { get; set; }

        public LightShaftComponent LightShaft { get; set; }
    }
}
