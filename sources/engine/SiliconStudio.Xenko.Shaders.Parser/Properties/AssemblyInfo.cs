// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Xenko.Shaders.Parser")]
[assembly: AssemblyTitle("SiliconStudio.Xenko.Shaders.Parser")]
[assembly: AssemblyDescription("Xenko shader parser assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Shaders.Parser.Serializers" + SiliconStudio.PublicKeys.Default)]
//[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Shaders.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine" + SiliconStudio.PublicKeys.Default)]
