// PkgCmdID.cs
// MUST match PkgCmdID.h
using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;

namespace SiliconStudio.Paradox.VisualStudio
{
    static class ParadoxPackageCmdIdList
    {
        public const uint cmdParadoxPlatformSelect =        0x100;
        public const uint cmdParadoxView =    0x101;
        public const uint cmdParadoxPlatformSelectList = 0x102;
        public const uint cmdParadoxCleanIntermediateAssetsSolutionCommand = 0x103;
        public const uint cmdParadoxCleanIntermediateAssetsProjectCommand = 0x104;
    };
}