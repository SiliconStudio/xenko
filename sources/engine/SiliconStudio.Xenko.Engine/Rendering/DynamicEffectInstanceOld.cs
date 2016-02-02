// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Collections;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering
{
    /// <summary>
    /// A dynamic effect instance updated by <see cref="DynamicEffectCompiler"/>.
    /// </summary>
    public abstract class DynamicEffectInstanceOld : IDisposable
    {
        internal DynamicEffectParameterUpdaterDefinition UpdaterDefinition;
        internal DynamicEffectParameterCollectionGroup ParameterCollectionGroup;

        // There is 2 states: compiling when CurrentlyCompilingEffect != null (will be glowing green, except if previously an error) or not
        internal Task<Effect> CurrentlyCompilingEffect;
        internal ParameterCollection CurrentlyCompilingUsedParameters;

        // There is 2 states: errors (will be glowing red) or not
        internal bool HasErrors;
        internal DateTime LastErrorCheck = DateTime.MinValue;

        protected DynamicEffectInstanceOld()
        {
        }

        /// <summary>
        /// Gets the effect currently being compiled.
        /// </summary>
        /// <value>The effect.</value>
        public Effect Effect { get; internal set; }

        /// <summary>
        /// Fills the parameter collections used by this instance.
        /// </summary>
        /// <param name="parameterCollections">The parameter collections.</param>
        public abstract void FillParameterCollections(ref FastListStruct<ParameterCollection> parameterCollections);

        public abstract void Dispose();
    }
}