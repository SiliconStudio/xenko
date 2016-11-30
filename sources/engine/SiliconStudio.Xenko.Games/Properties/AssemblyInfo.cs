// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Xenko.Games")]
[assembly: AssemblyTitle("SiliconStudio.Xenko.Games")]
[assembly: AssemblyDescription("Xenko Game Framework assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Games.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Input" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.UI" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Debugger" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Graphics.Regression" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.VirtualReality" + SiliconStudio.PublicKeys.Default)]