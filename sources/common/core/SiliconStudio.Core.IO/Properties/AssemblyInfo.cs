// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Core.IO")]
[assembly: AssemblyTitle("SiliconStudio.Core.IO")]
[assembly: AssemblyDescription("SiliconStudio Core IO assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Core.IO.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine.Step1" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Core.Tests" + SiliconStudio.PublicKeys.Default)]

#if SILICONSTUDIO_PLATFORM_IOS
[assembly: InternalsVisibleTo("SiliconStudioCoreTests" + SiliconStudio.PublicKeys.Default)]
#endif
