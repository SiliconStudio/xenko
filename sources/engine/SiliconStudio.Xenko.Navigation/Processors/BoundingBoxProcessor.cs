using System.Collections.Generic;
using System.Linq;
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

        protected override void OnSystemAdd()
        {
            base.OnSystemAdd();

            // TODO Plugins
            // This is the same kind of entry point as used in PhysicsProcessor
            var gameSystems = Services.GetServiceAs<IGameSystemCollection>();
            var navigationSystem = gameSystems.OfType<DynamicNavigationMeshSystem>().FirstOrDefault();
            if (navigationSystem == null)
            {
                navigationSystem = new DynamicNavigationMeshSystem(Services);
                gameSystems.Add(navigationSystem);
            }
        }
        
        public class BoundingBoxData
        {
        }
    }
}