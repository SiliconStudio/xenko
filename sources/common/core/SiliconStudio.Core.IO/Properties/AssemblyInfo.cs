// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Core.IO")]
[assembly: AssemblyTitle("SiliconStudio.Core.IO")]
[assembly: AssemblyDescription("SiliconStudio Core IO assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Core.IO.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Engine.Step1" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Core.Tests" + SiliconStudio.PublicKeys.Default)]

#if SILICONSTUDIO_PLATFORM_IOS
[assembly: InternalsVisibleTo("SiliconStudioCoreTests" + SiliconStudio.PublicKeys.Default)]
#endif