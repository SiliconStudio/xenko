// Guids.cs
// MUST match guids.h
using System;

namespace SiliconStudio.Paradox.VisualStudio
{
    internal static class GuidList
    {
        public const string guidParadox_VisualStudio_PackagePkgString = "c26b1ce9-bbab-497b-98ad-67e93a2037d1";
        public const string guidParadox_VisualStudio_PackageCmdSetString = "12225fdc-b608-43d7-9c75-e8b845984494";
        public const string guidToolWindowPersistanceString = "ddd10155-9f63-4694-95ce-c7ba2d74ad46";

        public const string guidParadox_VisualStudio_ShaderKeyFileGenerator = "BA7DB143-D0D6-4C7C-B545-1DCEDDB763FB";

        public const string guidParadox_VisualStudio_DataCodeGenerator = "52FCBA1A-42F7-4DBA-B543-C19E921EC203";

        public static readonly Guid guidParadox_VisualStudio_PackageCmdSet = new Guid(guidParadox_VisualStudio_PackageCmdSetString);

        public const string vsContextGuidVCSProject = "{FAE04EC1-301F-11D3-BF4B-00C04F79EFBC}";
    };
}