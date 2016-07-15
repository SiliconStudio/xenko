// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
#pragma warning disable 436 // The type 'type' in 'assembly' conflicts with the imported type 'type2' in 'assembly' (due to XenkoVersion being duplicated)
using System;
using System.Reflection;
using SiliconStudio;

[assembly:AssemblyCompany("Silicon Studio")]
[assembly:AssemblyCopyright("Copyright © 2011 Silicon Studio")]

[assembly:AssemblyTrademark("")]
[assembly:AssemblyCulture("")]

[assembly: AssemblyVersion(XenkoVersion.CurrentAssemblyAsText)]
[assembly: AssemblyFileVersion(XenkoVersion.CurrentAssemblyAsText)]

[assembly: AssemblyInformationalVersion(XenkoVersion.CurrentAsText)]

#if DEBUG
[assembly:AssemblyConfiguration("Debug")]
#else
[assembly:AssemblyConfiguration("Release")]
#endif

namespace SiliconStudio
{
    /// <summary>
    /// Internal version used to identify Xenko version.
    /// </summary>
    /// <remarks>
    /// Note: When modifying the version here, it must be modified also in the Xenko.xkpkg 
    /// </remarks>
    internal class XenkoVersion
    {
        /// <summary>
        /// The .NET current assembly version as text, not including pre-release (alpha, beta...) information.
        /// </summary>
        public const string CurrentAssemblyAsText = "1.7.4";

        /// <summary>
        /// The Store current version as text, including pre-release (alpha, beta...) information
        /// </summary>
        /// <remarks>
        /// Version number as described in http://docs.nuget.org/docs/reference/versioning
        /// When using pre-release (alpha, beta, rc...etc.) and because the order of the release is in alphabetical order,
        /// please use a double digit like alpha00 alpha01...etc. in order to make sure that we will follow the correct
        /// order for the versions.
        /// </remarks>
        public const string CurrentAsText = CurrentAssemblyAsText + "-beta"; 
    }

    partial class PublicKeys
    {
#if SILICONSTUDIO_SIGNED
        public const string Default = ", PublicKey=0024000004800000940000000602000000240000525341310004000001000100f5ddb3ad5749f108242f29cfaa2205e4a6b87c7444314975dc0fbed53b7d638c17f9540763e7355be932481737cd97a4104aecda872c4805ed9473c70c239d8798b22aefc351bb2cc387eb4391f31c53aeb0452b89433562b06754af8e460384656cd388fb9bbfef348292f9fb4ee6d07b74a8490923079865a60238df259cd2";
#else
        public const string Default = "";
#endif
    }
}
