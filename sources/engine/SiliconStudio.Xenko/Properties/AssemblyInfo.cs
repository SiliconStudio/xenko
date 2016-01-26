// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Xenko")]
[assembly: AssemblyTitle("SiliconStudio.Xenko")]
[assembly: AssemblyDescription("Xenko assembly.")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

// Make internals Xenko visible to Xenko assemblies
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics.ShaderCompiler" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Audio" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Games" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics.Regression" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine.NextGen" + SiliconStudio.PublicKeys.Default)]

#if !SILICONSTUDIO_SIGNED
[assembly: InternalsVisibleTo("SiliconStudio.ImageComparerService")]
#endif