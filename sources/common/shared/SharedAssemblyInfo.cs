// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Silicon Studio Corporation")]
[assembly: AssemblyCopyright("Copyright © 2011-2017 Silicon Studio Corporation")]

[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Special way to handle AssemblyVersion.
// The correct way to handle assembly version is to increment the AssemblyFileVersion
// for each build and only modify the AssemblyVersion when the changes are significant.
// Unfortunately, auto-increment of AssemblyFileVersion is not working, but only
// on AssemblyVersion. 
// So we are hacking it by using auto-increment on AssemblyVersion
// meaning that we have now a version x.y.z.XXXX. We then strip the XXXX from the 
// AssemblyVersion but we leave it in the AssemblyFileVersion. This is done in the 
// assembly processor
[assembly: AssemblyVersion("0.1.0.0")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly:AssemblyConfiguration("Release")]
#endif

[assembly: ComVisible(false)]

namespace SiliconStudio
{
    partial class PublicKeys
    {
#if SILICONSTUDIO_SIGNED
        public const string Default = ", PublicKey=0024000004800000940000000602000000240000525341310004000001000100f5ddb3ad5749f108242f29cfaa2205e4a6b87c7444314975dc0fbed53b7d638c17f9540763e7355be932481737cd97a4104aecda872c4805ed9473c70c239d8798b22aefc351bb2cc387eb4391f31c53aeb0452b89433562b06754af8e460384656cd388fb9bbfef348292f9fb4ee6d07b74a8490923079865a60238df259cd2";
#else
        public const string Default = "";
#endif
    }
}
