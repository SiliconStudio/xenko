using System;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("PhysicsSkinnedElementBase")]
    [Display(40, "PhysicsSkinnedElementBase")]
    public abstract class PhysicsSkinnedElementBase : PhysicsElementBase
    {
        /// <summary>
        /// Gets or sets the link (usually a bone).
        /// </summary>
        /// <value>
        /// The mesh's linked bone name
        /// </value>
        /// <userdoc>
        /// In the case of skinned mesh this must be the bone node name linked with this element. Cannot change during run-time.
        /// </userdoc>
        [DataMember(190)]
        public string NodeName { get; set; }
    }
}