using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Quantum
{
    [Flags]
    public enum SubHierarchyCloneFlags
    {
        /// <summary>
        /// No specific flag.
        /// </summary>
        None = 0,
        /// <summary>
        /// Clean any reference to an <see cref="IIdentifiable"/> object that is external to the sub-hierarchy.
        /// </summary>
        CleanExternalReferences = 1,
        /// <summary>
        /// Generates new identifiers for any <see cref="IIdentifiable"/> object that is internal to the sub-hierarchy.
        /// </summary>
        GenerateNewIdsForIdentifiableObjects = 2,
        /// <summary>
        /// Generates new identifiers for the <see cref="BasePart.InstanceId"/> of any part of the sub-hierarchy.
        /// </summary>
        GenerateNewBaseInstanceIds = 4,
        /// <summary>
        /// Do not apply overrides on the cloned sub-hierarchy.
        /// </summary>
        RemoveOverrides = 8,
    }
}
