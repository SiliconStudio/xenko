// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System.Linq;

using NUnit.Framework;

using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Graphics.Regression
{
    public class TestGraphicsDeviceManager : GraphicsDeviceManager
    {
        public TestGraphicsDeviceManager(GameBase game)
            : base(game)
        {
        }

        protected override bool IsPreferredProfileAvailable(GraphicsProfile[] preferredProfiles, out GraphicsProfile availableProfile)
        {
            if(!base.IsPreferredProfileAvailable(preferredProfiles, out availableProfile))
            {
                var minimumProfile = preferredProfiles.Min();
                Assert.Ignore("This test requires the '{0}' graphic profile. It has been ignored", minimumProfile);
            }

            return true;
        }
    }
}
