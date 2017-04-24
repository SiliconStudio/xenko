// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Xenko.Shaders")]
[assembly: AssemblyTitle("SiliconStudio.Xenko.Shaders")]
[assembly: AssemblyDescription("Xenko shaders core assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Shaders.Serializers" + SiliconStudio.PublicKeys.Default)]
