// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Linq;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Navigation;

namespace Gameplay
{
    public class EnableDynamicNavigation : StartupScript
    {
        public override void Start()
        {
            Game.GameSystems.OfType<DynamicNavigationMeshSystem>().FirstOrDefault().Enabled = true;
        }
    }
}
