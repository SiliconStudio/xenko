// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
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

#if !SILICONSTUDIO_SIGNED
[assembly: InternalsVisibleTo("SiliconStudio.ImageComparerService")]
#endif
