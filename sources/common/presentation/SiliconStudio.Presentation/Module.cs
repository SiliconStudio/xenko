// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Translation;
using SiliconStudio.Translation.Providers;

namespace SiliconStudio.Presentation
{
    internal class Module
    {
        [ModuleInitializer]
        public static void Initialize()
        {
            AssemblyRegistry.Register(typeof(Module).Assembly, AssemblyCommonCategories.Assets);
            // Initialize translation
            TranslationManager.Instance.RegisterProvider(new GettextTranslationProvider());
        }
    }
}
