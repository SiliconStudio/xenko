using System;

namespace SiliconStudio.Assets.Analysis
{
    [Flags]
    public enum BuildDependencyType
    {
        Runtime = 0x1,
        /// <summary>
        /// Only the asset is necessary
        /// </summary>
        CompileAsset = 0x2,
        /// <summary>
        /// The asset needs to be compiled because it will be Content.Load-ed by the command
        /// </summary>
        CompileContent = 0x4
    }
}