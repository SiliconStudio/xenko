// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using SiliconStudio.Core;
using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Quantum.Tests
{
    public class Module
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            ShadowObject.Enable = true;
        }
    }
}
