// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Particles
{
    [DataContract("ParticleTransform")]

    public class ParticleTransform
    {
        [DataMember(0)]
        [Display("Inherit position")]
        public bool InheritPosition { get; set; } = true;

        [DataMember(1)]
        public Vector3 Position { get; set; } = new Vector3(0, 0, 0);

        [DataMember(2)]
        [Display("Inherit rotation")]
        public bool InheritRotation { get; set; } = true;

        [DataMember(3)]
        public Quaternion Rotation { get; set; } = new Quaternion(0, 0, 0, 1);

        [DataMember(4)]
        [Display("Inherit scale")]
        public bool InheritScale { get; set; } = true;

        [DataMember(5)]
        public Vector3 Scale { get; set; } = new Vector3(1, 1, 1);

        [DataMemberIgnore]
        public Vector3 WorldPosition { get; private set; } = new Vector3(0, 0, 0);
        [DataMemberIgnore]
        public Quaternion WorldRotation { get; private set; } = new Quaternion(0, 0, 0, 1);
        [DataMemberIgnore]
        public Vector3 WorldScale { get; private set; } = new Vector3(1, 1, 1);


        public void SetParentTransform(ParticleTransform parent)
        {
            if (parent == null)
            {
                WorldPosition = Position;
                WorldRotation = Rotation;
                WorldScale = Scale;
                return;
            }

            WorldScale = (InheritScale) ? Scale * parent.WorldScale : Scale;

            WorldRotation = (InheritRotation) ? Rotation * parent.WorldRotation : Rotation;

            var offsetTranslation = Position * ((InheritScale) ? parent.WorldScale.X : 1f);

            if (InheritRotation)
            {
                parent.WorldRotation.Rotate(ref offsetTranslation);
            }

            WorldPosition = (InheritPosition) ? parent.WorldPosition + offsetTranslation : offsetTranslation;

        }
    
    }
}
