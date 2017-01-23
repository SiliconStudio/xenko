using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Models
{
    [DataContract("AnimationAssetDuration")]
    [Display("Clip duration")]
    public struct AnimationAssetDuration
    {
        [DataMember(-5)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the start frame of the animation.
        /// </summary>
        [DataMember(2)]
        [Display("Start frame")]
        public TimeSpan StartAnimationTime { get; set; }

        /// <summary>
        /// Gets or sets the end frame of the animation.
        /// </summary>
        [DataMember(4)]
        [Display("End frame")]
        public TimeSpan EndAnimationTime { get; set; }
    }
}
