// Copyright (c) 2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Navigation.Processors
{
    internal class BoundingBoxProcessor : EntityProcessor<NavigationBoundingBoxComponent, BoundingBoxProcessor.BoundingBoxData>
    {
        public delegate void CollectionChangedEventHandler(NavigationBoundingBoxComponent component);
        
        public ICollection<NavigationBoundingBoxComponent> BoundingBoxes => ComponentDatas.Keys;
        
        protected override BoundingBoxData GenerateComponentData(Entity entity, NavigationBoundingBoxComponent component)
        {
            return new BoundingBoxData();
        }

        protected override void OnSystemAdd()
        {
            base.OnSystemAdd();

            // TODO Plugins
            // This is the same kind of entry point as used in PhysicsProcessor
            var gameSystems = Services.GetSafeServiceAs<IGameSystemCollection>();
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
