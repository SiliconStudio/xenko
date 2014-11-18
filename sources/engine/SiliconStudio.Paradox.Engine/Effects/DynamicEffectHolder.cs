// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects
{
    public abstract class DynamicEffectHolder
    {
        internal EffectParameterUpdaterDefinition UpdaterDefinition;

        protected DynamicEffectHolder()
        {
        }

        /// <summary>
        /// Gets the effect.
        /// </summary>
        /// <value>The effect.</value>
        public Effect Effect { get; internal set; }

        protected internal abstract void FillParameterCollections(IList<ParameterCollection> parameterCollections);
    }
}