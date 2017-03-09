using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Navigation.Processors
{
    internal class BoundingBoxProcessor : EntityProcessor<NavigationMeshBoundingBox, BoundingBoxProcessor.BoundingBoxData>
    {
        public delegate void CollectionChangedEventHandler(NavigationMeshBoundingBox component);
        
        public ICollection<NavigationMeshBoundingBox> BoundingBoxes => ComponentDatas.Keys;
        
        protected override BoundingBoxData GenerateComponentData(Entity entity, NavigationMeshBoundingBox component)
        {
            return new BoundingBoxData();
        }
        
        protected override void OnEntityComponentAdding(Entity entity, NavigationMeshBoundingBox component, BoundingBoxData data)
        {
            base.OnEntityComponentAdding(entity, component, data);
        }

        protected override void OnEntityComponentRemoved(Entity entity, NavigationMeshBoundingBox component, BoundingBoxData data)
        {
            base.OnEntityComponentRemoved(entity, component, data);
        }

        protected override void OnSystemAdd()
        {
            base.OnSystemAdd();

            // TODO Plugins
            // This is the same kind of entry point as used in PhysicsProcessor
            var navigationSystem = Services.GetServiceAs<DynamicNavigationMeshSystem>();
            if (navigationSystem == null)
            {
                navigationSystem = new DynamicNavigationMeshSystem(Services);
                var gameSystems = Services.GetServiceAs<IGameSystemCollection>();
                gameSystems.Add(navigationSystem);
            }
        }
        
        public class BoundingBoxData
        {
        }
    }
}