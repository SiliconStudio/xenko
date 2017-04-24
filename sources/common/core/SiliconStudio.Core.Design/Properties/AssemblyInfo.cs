// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyProduct("SiliconStudio.Core.IO")]
[assembly: AssemblyTitle("SiliconStudio.Core.IO")]
[assembly: AssemblyDescription("SiliconStudio Core IO assembly")]

[assembly: InternalsVisibleTo("SiliconStudio.Core.Design.Serializers")]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine")]
[assembly: InternalsVisibleTo("SiliconStudio.Xenko.Engine.Step1")]
[assembly: InternalsVisibleTo("SiliconStudio.Core.Tests")]
[assembly: InternalsVisibleTo("SiliconStudio.Core.Design.Tests")]
[assembly: InternalsVisibleTo("SiliconStudio.Presentation.Tests")]
// looks like whenever we open the generated iOS solution with visual studio, it removes the dot in the assembly name -_-
#if SILICONSTUDIO_PLATFORM_IOS
[assembly: InternalsVisibleTo("SiliconStudioCoreTests")]
#endif
