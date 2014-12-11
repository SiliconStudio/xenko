// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SiliconStudio.Paradox.Effects
{
    /// <summary>
    /// Various extension methods to quickly get and set values in EffectVariable.
    /// </summary>
    public static class EffectParameterExtensions
    {
        public static void RegisterParameter(this ParameterCollection parameterCollection, ParameterKey parameterKey, bool addDependencies = true)
        {
            var metaData = parameterKey.Metadatas.OfType<ParameterKeyValueMetadata>().FirstOrDefault();
            
            if (metaData == null)
                throw new ArgumentException("ParameterKey must be declared with metadata", "parameterKey");

            if (!parameterCollection.ContainsKey(parameterKey))
            {
                metaData.SetupDefaultValue(parameterCollection, parameterKey, addDependencies);
            }
        }
    }
}
