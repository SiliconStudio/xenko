// Copyright (c) 2014-2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Xenko.Particles.Spawners;

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

        [DataMember(11)]
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
        /// The parent seed offset is used to determine which particle from the pool should be picked as a parent in case there is no control group
        /// </summary>
        /// <userdoc>
        /// The parent seed offset is used to determine which particle from the pool should be picked as a parent in case there is no control group
        /// </userdoc>
        [DataMember(12)]
        [Display("Parent Offset")]
        public uint ParentSeedOffset { get; set; } = 0;

        /// <summary>
        /// Some initializers require fine control between parent and child emitters. Use the control group to assign such meta-fields.
        /// </summary>
        [DataMember(13)]
        [Display("Spawn Control Group")]
        public ParentControlFlag ParentControlFlag { get; set; } = ParentControlFlag.Group00;

        /// <summary>
        /// Gets a field accessor to the parent emitter's spawn control field, if it exists
        /// </summary>
        /// <returns></returns>
        protected ParticleFieldAccessor<ParticleChildrenAttribute> GetSpawnControlField()
        {
            var groupIndex = (int)ParentControlFlag;
            if (groupIndex >= ParticleFields.ChildrenFlags.Length)
                return ParticleFieldAccessor<ParticleChildrenAttribute>.Invalid();

            return Parent?.Pool?.GetField(ParticleFields.ChildrenFlags[groupIndex]) ?? ParticleFieldAccessor<ParticleChildrenAttribute>.Invalid();
        } 

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

