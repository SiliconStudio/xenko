// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

using SiliconStudio.Assets.Compiler;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Data;
using SiliconStudio.Paradox.Shaders.Compiler;

namespace SiliconStudio.Paradox.Assets.Effect
{
    abstract class CompilerParameterGeneratorBase : ICompilerParametersGenerator
    {
        public virtual int GeneratorPriority { get; private set; }
        
        public abstract IEnumerable<CompilerParameters> Generate(AssetCompilerContext context, CompilerParameters baseParameters, ILogger log);
        
        /// <summary>
        /// Adds the parameters from the ParameterCollectionData to the destination ParameterCollection. Excludes GraphicsResourceBase parameters.
        /// </summary>
        /// <param name="sourceParameters">The source ParameterCollectionData.</param>
        /// <param name="destParameters">The destination ParameterCollection.</param>
        protected static void AddToParameters(ParameterCollectionData sourceParameters, ParameterCollection destParameters)
        {
            if (sourceParameters != null)
            {
                foreach (var keyValue in sourceParameters)
                {
                    // only keep what can be assigned
                    if (keyValue.Value.GetType().IsAssignableFrom(keyValue.Key.PropertyType))
                        destParameters.SetObject(keyValue.Key, keyValue.Key.ConvertValue(keyValue.Value));
                }
            }
        }
    }
}
