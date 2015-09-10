using System;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("PhysicsSkinnedElementBase")]
    [Display(40, "PhysicsSkinnedElementBase")]
    public abstract class PhysicsSkinnedElementBase : PhysicsElementBase
    {
        private string nodeName;

        /// <summary>
        /// Gets or sets the link (usually a bone).
        /// </summary>
        /// <value>
        /// The mesh's linked bone name
        /// </value>
        /// <userdoc>
        /// In the case of skinned mesh this must be the bone node name linked with this element.
        /// </userdoc>
        [DataMember(50)]
        public string NodeName
        {
            get
            {
                return nodeName;
            }
            set
            {
                if (InternalCollider != null)
                {
                    throw new Exception("Cannot change NodeName when the entity is already in the scene.");
                }

                nodeName = value;
            }
        }
    }
}