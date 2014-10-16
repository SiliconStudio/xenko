// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;

namespace SiliconStudio.Paradox
{
    public abstract class GameSystem : GameSystemBase
    {
        protected GameSystem(IServiceRegistry registry) : base(registry)
        {
            Input = Services.GetSafeServiceAs<InputManager>();
            Entities = Services.GetSafeServiceAs<EntitySystem>();
        }

        public InputManager Input { get; private set; }

        public EntitySystem Entities { get; internal set; }
    }
}