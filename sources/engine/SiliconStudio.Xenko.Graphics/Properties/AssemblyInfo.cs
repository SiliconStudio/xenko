// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Xenko.Graphics")]
[assembly: AssemblyTitle("SiliconStudio.Xenko.Graphics")]
[assembly: AssemblyDescription("Xenko Graphics assembly.")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

// Make internals Xenko visible to Xenko assemblies
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics.ShaderCompiler" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine.Step1" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Games" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.UI" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudioXenkoGraphicsTests" + SiliconStudio.PublicKeys.Default)] // iOS removes dot
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudioXenkoEngineTests" + SiliconStudio.PublicKeys.Default)] // iOS removes dot
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics.Regression" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Assets" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.VirtualReality" + SiliconStudio.PublicKeys.Default)]

#if !SILICONSTUDIO_SIGNED
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Assets.Presentation")]
#endif
