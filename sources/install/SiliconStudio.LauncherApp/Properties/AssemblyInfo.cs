// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SiliconStudio.LauncherApp")]
[assembly: AssemblyDescription("SiliconStudio launcher application")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("SiliconStudio Corp")]
[assembly: AssemblyProduct("SiliconStudio.LauncherApp")]
[assembly: AssemblyCopyright("Copyright © SiliconStudio Corp")]
[assembly: AssemblyTrademark("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// Assembly version for the launcher, used to generate the appropriate nuget package
[assembly: AssemblyVersion("1.0.5.0")]
[assembly: AssemblyInformationalVersion("1.0.5")]

[assembly: NeutralResourcesLanguage("en-US")]