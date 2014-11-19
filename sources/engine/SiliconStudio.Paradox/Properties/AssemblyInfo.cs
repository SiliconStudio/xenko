// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Paradox")]
[assembly: AssemblyTitle("SiliconStudio.Paradox")]
[assembly: AssemblyDescription("Paradox assembly.")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

// Make internals Paradox visible to Paradox assemblies
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Graphics.ShaderCompiler" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Graphics" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Audio" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Games" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Graphics.Regression" + SiliconStudio.PublicKeys.Default)]

#if !SILICONSTUDIO_SIGNED
[assembly: InternalsVisibleTo("SiliconStudio.ImageComparerService")]
#endif