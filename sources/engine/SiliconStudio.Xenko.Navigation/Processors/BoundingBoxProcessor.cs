// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Navigation.Processors
{
    internal class BoundingBoxProcessor : EntityProcessor<NavigationBoundingBox, BoundingBoxProcessor.BoundingBoxData>
    {
        public delegate void CollectionChangedEventHandler(NavigationBoundingBox component);
        
        public ICollection<NavigationBoundingBox> BoundingBoxes => ComponentDatas.Keys;
        
        protected override BoundingBoxData GenerateComponentData(Entity entity, NavigationBoundingBox component)
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
