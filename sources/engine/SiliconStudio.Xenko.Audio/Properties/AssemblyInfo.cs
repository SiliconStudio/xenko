// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Xenko.Audio")]
[assembly: AssemblyTitle("SiliconStudio.Xenko.Audio")]
[assembly: AssemblyDescription("Xenko Audio Framework assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Audio.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Audio.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudioXenkoAudioTests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Assets" + SiliconStudio.PublicKeys.Default)]
