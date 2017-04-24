// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Assets.Analysis;
using SiliconStudio.Assets.Templates;
using SiliconStudio.Assets.Tracking;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;

namespace SiliconStudio.Assets
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            // Shadow object is always enabled when we are using assets, so we force it here
            ShadowObject.Enable = true;

            // Make sure that this assembly is registered
            AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);

            AssetYamlSerializer.Default.PrepareMembers += SourceHashesHelper.AddSourceHashesMember;
        }
    }
}
