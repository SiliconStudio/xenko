// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Data;

namespace SiliconStudio.Paradox.Engine
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            AssemblyRegistry.Register(typeof(Module).GetTypeInfo().Assembly, AssemblyCommonCategories.Assets);

            // Register EntityReferenceDataConverter
            ConverterContext.RegisterConverter(new EntityReferenceDataConverter());
        }
    }
}