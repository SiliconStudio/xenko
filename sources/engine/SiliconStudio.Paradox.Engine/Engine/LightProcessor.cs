// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine
{
    public class LightProcessor : EntityProcessor<LightComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightProcessor" /> class.
        /// </summary>
        public LightProcessor()
            : base(new PropertyKey[] { LightComponent.Key })
        {
        }

        public Dictionary<Entity, LightComponent> Lights
        {
            get { return enabledEntities; }
        }

        /// <inheritdoc/>
        protected override LightComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get(LightComponent.Key);
        }

        /// <summary>
        /// Updates all the <see cref="TransformationComponent.WorldMatrix"/>.
        /// </summary>
        /// <param name="time"></param>
        public override void Draw(GameTime time)
        {
        }
    }
}