// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Particles.Initializers
{
    /// <summary>
    /// Base class for initializers which reference a parent particle emitter
    /// </summary>
    [DataContract("ParticleChildInitializer")]
    public abstract class ParticleChildInitializer : ParticleInitializer
    {
        /// <summary>
        /// Referenced parent emitter
        /// </summary>
        [DataMemberIgnore]
        protected ParticleEmitter Parent;

        private string parentName;

        /// <summary>
        /// <c>true</c> is the parent's name has changed or the particle system has been invalidated
        /// </summary>
        private bool isParentNameDirty = true;

        [DataMember(2)]
        [Display("Parent emitter")]
        public string ParentName
        {
            get { return parentName; }
            set
            {
                parentName = value;
                isParentNameDirty = true;
            }
        }

        /// <summary>
        /// The parent seed offset is used to determine which particle from the pool should be picked as a parent
        /// </summary>
        /// <userdoc>
        /// The parent seed offset is used to determine which particle from the pool should be picked as a parent
        /// </userdoc>
        [DataMember(3)]
        [Display("Parent Offset")]
        public uint ParentSeedOffset { get; set; } = 0;

        /// <inheritdoc />
        public override void SetParentTrs(ParticleTransform transform, ParticleSystem parentSystem)
        {
            base.SetParentTrs(transform, parentSystem);

            if (isParentNameDirty)
            {
                Parent = parentSystem?.GetEmitterByName(ParentName);
                isParentNameDirty = false;
            }
        }

        /// <inheritdoc />
        public override void InvalidateRelations()
        {
            base.InvalidateRelations();

            Parent = null;
            isParentNameDirty = true;
        }
    }
}

