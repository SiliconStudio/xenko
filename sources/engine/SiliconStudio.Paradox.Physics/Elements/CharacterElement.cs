using System;
using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Physics
{
    [DataContract("CharacterElement")]
    [Display(40, "Character")]
    public class CharacterElement : PhysicsElementBase, IPhysicsElement
    {
        public CharacterElement()
        {
            StepHeight = 0.1f;
        }

        public override Types Type
        {
            get { return Types.CharacterController; }
        }

        private float stepHeight;

        /// <summary>
        /// Gets or sets the height of the character step.
        /// </summary>
        /// <value>
        /// The height of the character step.
        /// </value>
        /// <userdoc>
        /// Only valid for CharacterController type, describes the max slope height a character can climb.
        /// </userdoc>
        [DataMember(60)]
        [DefaultValue(0.1f)]
        public float StepHeight
        {
            get
            {
                return stepHeight;
            }
            set
            {
                if (InternalCollider != null)
                {
                    throw new Exception("Cannot change StepHeight when the entity is already in the scene.");
                }

                stepHeight = value;
            }
        }
    }
}