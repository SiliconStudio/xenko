using System.ComponentModel;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// An <see cref="EntityComponent"/> that can be enabled and disabled.
    /// </summary>
    [DataContract]
    public abstract class ActivableEntityComponent : EntityComponent
    {
        private bool enabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="EntityComponent"/> is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember(-10)]
        [DefaultValue(true)]
        public bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }
    }
}