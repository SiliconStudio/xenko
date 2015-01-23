// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core;
using SiliconStudio.Paradox.EntityModel;

namespace SiliconStudio.Paradox.Effects.Skyboxes
{
    /// <summary>
    /// A default entity processor for <see cref="SkyboxComponent"/>.
    /// </summary>
    public class SkyboxProcessor : EntityProcessor<SkyboxComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkyboxProcessor" /> class.
        /// </summary>
        public SkyboxProcessor()
            : base(new PropertyKey[] { SkyboxComponent.Key })
        {
        }

        public Dictionary<Entity, SkyboxComponent> Skyboxes
        {
            get { return enabledEntities; }
        }

        /// <inheritdoc/>
        protected override SkyboxComponent GenerateAssociatedData(Entity entity)
        {
            return entity.Get(SkyboxComponent.Key);
        }
    }
}