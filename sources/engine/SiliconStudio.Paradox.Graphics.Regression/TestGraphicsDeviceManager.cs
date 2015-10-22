// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Linq;

using NUnit.Framework;

using SiliconStudio.Paradox.Games;

namespace SiliconStudio.Paradox.Graphics.Regression
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