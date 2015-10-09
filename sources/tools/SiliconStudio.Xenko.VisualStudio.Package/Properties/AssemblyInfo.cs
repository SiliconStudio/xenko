// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SiliconStudio.Paradox.VisualStudio;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Paradox.VisualStudio.Package")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("SiliconStudio")]
[assembly: AssemblyProduct("Paradox.VisualStudio.Package")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]   
[assembly: ComVisible(false)]     
[assembly: CLSCompliant(false)]
[assembly: NeutralResourcesLanguage("en-US")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion(ParadoxPackage.Version)]
[assembly: AssemblyFileVersion(ParadoxPackage.Version)]

[assembly: InternalsVisibleTo("Paradox.VisualStudio.Package_IntegrationTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100eff3831ac0cd83afc7961adcb2b01fca332a4e93d227d7e8d644fc3275456f611d2c01e586e760d62a2fc83fc79995fb2aebe9859657cfb725d281d992ddc1ba26d2e00986ee0042f0fbf99594d5d0ba83ba8b9a9d0fd6fe1fdd6297c044a7c7b568c094a1c7409a2308d4c9f73ce2f73ec3e21e42d2b64e8a1a088ef53e8ec6")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1017:MarkAssembliesWithComVisible")]

[assembly: InternalsVisibleTo("SiliconStudio.Paradox.VisualStudio.Commands")]

