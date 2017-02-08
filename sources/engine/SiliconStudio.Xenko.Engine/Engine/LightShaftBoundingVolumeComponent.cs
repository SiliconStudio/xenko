using System.Collections.Generic;
using System.Windows.Media.Media3D;
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

                foreach (var affectedComponent in pair.Key.AffectedComponents)
                {
                    if (affectedComponent == null)
                        continue;

                    List<LightShaftBoundingVolumeData> data;
                    if (!volumesPerLightShaft.TryGetValue(affectedComponent, out data))
                        volumesPerLightShaft.Add(affectedComponent, 
                            data = new List<LightShaftBoundingVolumeData>());

                    data.Add(new LightShaftBoundingVolumeData
                    {
                        World = world,
                        Model = pair.Key.Model
                    });
                }
            }
        }

        public IEnumerable<LightShaftBoundingVolumeData> GetBoundingVolumesForComponent(LightShaftComponent component)
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
    public class LightShaftBoundingVolumeComponent : EntityComponent
    {
        [DataMember(0)] public bool Enabled = true;

        [DataMember(5)]
        public Model Model { get; set; }

        [DataMember(10)]
        public List<LightShaftComponent> AffectedComponents { get; } = new List<LightShaftComponent>();
    }
}