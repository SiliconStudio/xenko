// Copyright (c) 2011 Silicon Studio
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Core.Mathematics")]
[assembly: AssemblyTitle("SiliconStudio.Core.Mathematics")]
[assembly: AssemblyDescription("SiliconStudio Core Mathematics assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Core.Mathematics.Serializers" + SiliconStudio.PublicKeys.Default)]