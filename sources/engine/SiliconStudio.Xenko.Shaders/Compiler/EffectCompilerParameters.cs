using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Shaders.Compiler
{
    [DataContract]
    public struct EffectCompilerParameters
    {
        /// <summary>
        /// Gets or sets the priority (in case this compile is scheduled in a custom async pool)
        /// </summary>
        /// <value>
        /// The priority.
        /// </value>
        [DataMemberIgnore]
        public int TaskPriority { get; set; }
    }
}