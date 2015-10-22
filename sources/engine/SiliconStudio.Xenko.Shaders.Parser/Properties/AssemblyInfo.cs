// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Paradox.Shaders.Parser")]
[assembly: AssemblyTitle("SiliconStudio.Paradox.Shaders.Parser")]
[assembly: AssemblyDescription("Paradox shader parser assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Shaders.Parser.Serializers" + SiliconStudio.PublicKeys.Default)]
//[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Shaders.Tests" + SiliconStudio.PublicKeys.Default)]
