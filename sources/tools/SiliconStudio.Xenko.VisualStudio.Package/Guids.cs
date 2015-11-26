// Guids.cs
// MUST match guids.h
using System;

namespace SiliconStudio.Xenko.VisualStudio
{
    internal static class GuidList
    {
        public const string guidXenko_VisualStudio_PackagePkgString = "B0B8FEB1-7B83-43FC-9FC0-70065DDB80A1";
        public const string guidXenko_VisualStudio_PackageCmdSetString = "9428DB93-BFEA-4115-8D4A-40B047166E61";
        public const string guidToolWindowPersistanceString = "ddd10155-9f63-4694-95ce-c7ba2d74ad46";

        public const string guidXenko_VisualStudio_ShaderKeyFileGenerator = "B50E6ECE-B11F-477B-A8E1-1E60E0531A53";

        public const string guidXenko_VisualStudio_DataCodeGenerator = "22555301-D58A-4D71-9DAB-B2552CC3DE0E";

        public static readonly Guid guidXenko_VisualStudio_PackageCmdSet = new Guid(guidXenko_VisualStudio_PackageCmdSetString);

        public const string vsContextGuidVCSProject = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";
    };
}