// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Reflection;

namespace SiliconStudio.Paradox.Assets.Materials
{
    /// <summary>
    /// An interface used when visiting <see cref="IMaterialFeature"/>and <see cref="IMaterialComputeColor"/> members of a material
    /// object.
    /// </summary>
    public interface IMaterialFeatureVisitor
    {
        /// <summary>
        /// Visits a <see cref="IMaterialFeature"/> defined in the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="member">The member.</param>
        /// <param name="feature">The feature.</param>
        void Visit(object instance, IMemberDescriptor member, IMaterialFeature feature);

        /// <summary>
        /// Visits a <see cref="IMaterialComputeColor"/> defined in the specified instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="member">The member.</param>
        /// <param name="computeColor">The computeColor.</param>
        /// <param name="streamAttribute">The stream attribute attached to this .</param>
        void Visit(object instance, IMemberDescriptor member, IMaterialComputeColor computeColor, MaterialStreamAttribute streamAttribute);
    }
}