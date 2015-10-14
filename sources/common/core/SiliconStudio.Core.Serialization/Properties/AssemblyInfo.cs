// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Core.Serialization")]
[assembly: AssemblyTitle("SiliconStudio.Core.Serialization")]
[assembly: AssemblyDescription("SiliconStudio Core Serialization assembly")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

// Make internals Xenko.Framework.visible to all Xenko.Framework.assemblies
[assembly: InternalsVisibleTo("SiliconStudio.Core.Serialization.Serializers" + SiliconStudio.PublicKeys.Default)]