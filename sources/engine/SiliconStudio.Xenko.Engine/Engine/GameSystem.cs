// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core;
using SiliconStudio.Xenko.Games;

namespace SiliconStudio.Xenko.Engine
{
    public abstract class GameSystem : GameSystemBase
    {
        protected GameSystem(IServiceRegistry registry) : base(registry)
        {
        }

        /// <summary>
        /// Gets the <see cref="Game"/> associated with this <see cref="GameSystemBase"/>. This value can be null in a mock environment.
        /// </summary>
        /// <value>The game.</value>
        /// <remarks>This value can be null</remarks>
        public new Game Game => (Game)base.Game;
    }
}
