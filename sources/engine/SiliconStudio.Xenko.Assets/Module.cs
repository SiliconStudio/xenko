// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.IO;
using System.Reflection.Emit;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Templates;
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Rendering;
using SiliconStudio.Xenko.Rendering.Skyboxes;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Assets
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            // Register solution platforms
            XenkoConfig.RegisterSolutionPlatforms();

            AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);
            AssemblyRegistry.Register(typeof(ParameterKeys).Assembly, AssemblyCommonCategories.Assets);
            AssemblyRegistry.Register(typeof(SkyboxComponent).Assembly, AssemblyCommonCategories.Assets);
            AssemblyRegistry.Register(typeof(Texture).Assembly, AssemblyCommonCategories.Assets);
            AssemblyRegistry.Register(typeof(ShaderClassSource).Assembly, AssemblyCommonCategories.Assets);

            // Add AllowMultipleComponentsAttribute on EntityComponent yaml proxy (since there might be more than one)
            UnloadableObjectInstantiator.ProcessProxyType += ProcessEntityComponent;
        }

        private static void ProcessEntityComponent(Type baseType, TypeBuilder typeBuilder)
        {
            if (typeof(EntityComponent).IsAssignableFrom(baseType))
            {
                // Add AllowMultipleComponentsAttribute on EntityComponent yaml proxy (since there might be more than one)
                var allowMultipleComponentsAttributeCtor = typeof(AllowMultipleComponentsAttribute).GetConstructor(Type.EmptyTypes);
                var allowMultipleComponentsAttribute = new CustomAttributeBuilder(allowMultipleComponentsAttributeCtor, new object[0]);
                typeBuilder.SetCustomAttribute(allowMultipleComponentsAttribute);
            }
        }
    }
}