// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Paradox.Physics")]
[assembly: AssemblyTitle("SiliconStudio.Paradox.Physics")]
[assembly: AssemblyDescription("Paradox physics core assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Engine" + SiliconStudio.PublicKeys.Default)]
