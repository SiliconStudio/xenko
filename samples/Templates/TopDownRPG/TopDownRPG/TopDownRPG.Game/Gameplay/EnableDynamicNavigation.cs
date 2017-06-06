// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Collections.Specialized;
using System.Linq;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Navigation;

namespace Gameplay
{
    public class EnableDynamicNavigation : StartupScript
    {
        public override void Start()
        {
            var dynamicNavigationMeshSystem = Game.GameSystems.OfType<DynamicNavigationMeshSystem>().FirstOrDefault();

            // Wait for the dynamic navigation to be registered
            if(dynamicNavigationMeshSystem == null)
                Game.GameSystems.CollectionChanged += GameSystemsOnCollectionChanged;
        }

        public override void Cancel()
        {
            Game.GameSystems.CollectionChanged -= GameSystemsOnCollectionChanged;
        }

        private void GameSystemsOnCollectionChanged(object sender, TrackingCollectionChangedEventArgs trackingCollectionChangedEventArgs)
        {
            if (trackingCollectionChangedEventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                var dynamicNavigationMeshSystem = trackingCollectionChangedEventArgs.Item as DynamicNavigationMeshSystem;
                if (dynamicNavigationMeshSystem != null)
                {
                    dynamicNavigationMeshSystem.Enabled = true;

                    // No longer need to listen to changes
                    Game.GameSystems.CollectionChanged -= GameSystemsOnCollectionChanged;
                }
            }
        }
    }
}
