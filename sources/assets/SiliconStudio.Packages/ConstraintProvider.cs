// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Collection of constraints associated to some packages expressed as version ranges.
    /// </summary>
    public class ConstraintProvider
    {
        /// <summary>
        /// Store <see cref="PackageVersionRange"/> constraints associated to a given package.
        /// </summary>
        internal readonly Dictionary<string, PackageVersionRange> Constraints = new Dictionary<string, PackageVersionRange>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Does current instance have constraints?
        /// </summary>
        public bool HasConstraints => Constraints.Count > 0;

        /// <summary>
        /// Add constraint <paramref name="range"/> to package ID <paramref name="packageId"/>.
        /// </summary>
        /// <param name="packageId">Package on which constraint <paramref name="range"/> will be applied.</param>
        /// <param name="range">Range of constraint.</param>
        public void AddConstraint(string packageId, PackageVersionRange range)
        {
            Constraints[packageId] = range;
        }

        /// <summary>
        /// Retrieve constraint associated with <paramref name="packageId"/> if any.
        /// </summary>
        /// <param name="packageId">Id of package being queried.</param>
        /// <returns>Constraint if any, null otherwise.</returns>
        internal PackageVersionRange GetConstraint(string packageId)
        {
            PackageVersionRange versionRange;
            if (Constraints.TryGetValue(packageId, out versionRange))
            {
                return versionRange;
            }
            return null;
        }
    }
}
