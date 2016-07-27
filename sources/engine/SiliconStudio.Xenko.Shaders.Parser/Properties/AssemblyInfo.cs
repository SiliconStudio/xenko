// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Xenko.Shaders.Parser")]
[assembly: AssemblyTitle("SiliconStudio.Xenko.Shaders.Parser")]
[assembly: AssemblyDescription("Xenko shader parser assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Shaders.Parser.Serializers" + SiliconStudio.PublicKeys.Default)]
//[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Shaders.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine" + SiliconStudio.PublicKeys.Default)]