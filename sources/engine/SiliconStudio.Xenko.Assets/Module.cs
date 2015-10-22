// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Templates;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Skyboxes;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Assets
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            // Register solution platforms
            ParadoxConfig.RegisterSolutionPlatforms();

            AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);
            AssemblyRegistry.Register(typeof(ParameterKeys).Assembly, AssemblyCommonCategories.Assets);
            AssemblyRegistry.Register(typeof(SkyboxComponent).Assembly, AssemblyCommonCategories.Assets);
            AssemblyRegistry.Register(typeof(Texture).Assembly, AssemblyCommonCategories.Assets);
            AssemblyRegistry.Register(typeof(ShaderClassSource).Assembly, AssemblyCommonCategories.Assets);
        }
    }
}