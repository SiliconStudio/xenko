// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;

namespace SiliconStudio.Xenko.Particles.Updaters.FieldShapes
{
    [DataContract("FieldFalloff")]
    [Display("Falloff")]
    public class FieldFalloff
    {
        /// <summary>
        /// The strength of the force in the center of the bounding shape.
        /// </summary>
        /// <userdoc>
        /// The strength of the force in the center of the bounding shape.
        /// </userdoc>
        [DataMember(10)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Strength inside")]
        public float StrengthInside { get; set; } = 1f;

        /// <summary>
        /// After this relative distance from the center, the force strength will start to change
        /// </summary>
        /// <userdoc>
        /// After this relative distance from the center, the force strength will start to change
        /// </userdoc>
        [DataMember(20)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Falloff start")]
        public float FalloffStart { get; set; } = 0.1f;

        /// <summary>
        /// The strength of the force outside the bounding shape.
        /// </summary>
        /// <userdoc>
        /// The strength of the force outside the bounding shape.
        /// </userdoc>
        [DataMember(30)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Strength outside")]
        public float StrengthOutside { get; set; } = 0f;


        /// <summary>
        /// After this relative distance from the center, the force strength will be equal to [Strength outside]
        /// </summary>
        /// <userdoc>
        /// After this relative distance from the center, the force strength will be equal to [Strength outside]
        /// </userdoc>
        [DataMember(40)]
        [DataMemberRange(0, 1, 0.001, 0.1)]
        [Display("Falloff end")]
        public float FalloffEnd { get; set; } = 0.9f;

        /// <summary>
        /// Get interpolated strength based on relative distance from the center (lerp)
        /// </summary>
        /// <param name="inDistance"></param>
        /// <returns></returns>
        public float GetStrength(float inDistance)
        {
            if (inDistance <= FalloffStart)
                return StrengthInside;

            if (inDistance >= FalloffEnd)
                return StrengthOutside;

            var lerp = (inDistance - FalloffStart) / (FalloffEnd / FalloffStart);
            return StrengthInside + (StrengthOutside - StrengthInside) * lerp;
        }
    }
}
