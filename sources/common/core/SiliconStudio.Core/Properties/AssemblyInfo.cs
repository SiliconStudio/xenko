// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Core")]
[assembly: AssemblyTitle("SiliconStudio.Core")]
[assembly: AssemblyDescription("Core assembly for all SiliconStudio assemblies.")]

#pragma warning disable 436 // SiliconStudio.PublicKeys is defined in multiple assemblies

// Make internals Paradox.Framework.visible to all Paradox.Framework.assemblies
[assembly: InternalsVisibleTo("SiliconStudio.Core.Serializers" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Core.IO" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.UI" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Engine" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Graphics" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Paradox.Games" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudio.Core.Tests" + SiliconStudio.PublicKeys.Default)]
[assembly: InternalsVisibleTo("SiliconStudioCoreTests" + SiliconStudio.PublicKeys.Default)]