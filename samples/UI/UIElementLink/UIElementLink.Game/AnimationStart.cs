// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Animations;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Engine;

namespace UIElementLink
{
    [DataContract]
    public class PlayAnimation
    {
        public AnimationClip Clip;
        public AnimationBlendOperation BlendOperation = AnimationBlendOperation.LinearBlend;
        public double StartTime = 0;
    }
    
    /// <summary>
    /// Script which starts a few animations on its entity
    /// </summary>
    public class AnimationStart : StartupScript
    {
        /// <summary>
        /// Al list of animations to be loaded when the script starts
        /// </summary>
        public readonly List<PlayAnimation> Animations = new List<PlayAnimation>();

        public override void Start()
        {
			var animComponent = Entity.GetOrCreate<AnimationComponent>();
        
            if (animComponent != null)
                PlayAnimations(animComponent);

            // Destroy this script since it's no longer needed
            Entity.Remove(this);
        }

        private void PlayAnimations(AnimationComponent animComponent)
        {
            foreach (var anim in Animations)
            {
                if (anim.Clip != null)
                    animComponent.Add(anim.Clip, anim.StartTime, anim.BlendOperation);
            }

            Animations.Clear();
        }
    }
}
