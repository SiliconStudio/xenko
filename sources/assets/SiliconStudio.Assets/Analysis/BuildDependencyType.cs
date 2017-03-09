using System;

namespace SiliconStudio.Assets.Analysis
{
    [Flags]
    public enum BuildDependencyType
    {
        Runtime = 0x1,
        CompileAsset = 0x2,
        CompileContent = 0x4
    }
}