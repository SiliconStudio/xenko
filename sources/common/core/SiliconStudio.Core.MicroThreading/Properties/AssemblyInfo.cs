// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Core.MicroThreading")]
[assembly: AssemblyTitle("SiliconStudio.Core.MicroThreading")]
[assembly: AssemblyDescription("SiliconStudio Core Microthreading assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

// Make internals Xenko.Framework.visible to all Xenko.Framework.assemblies
[assembly: InternalsVisibleTo("SiliconStudio.Core.MicroThreading.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Debugger" + SiliconStudio.PublicKeys.Default)]
