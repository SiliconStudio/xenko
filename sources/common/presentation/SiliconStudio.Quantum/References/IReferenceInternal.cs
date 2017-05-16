// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Quantum.References
{
    internal interface IReferenceInternal : IReference
    {
        /// <summary>
        /// Refreshes this reference by making point to the proper target nodes. If no node exists yet for some of the target, they will be created
        /// using the given node factory.
        /// </summary>
        /// <param name="ownerNode">The node owning this reference.</param>
        /// <param name="nodeContainer">The node container containing the <paramref name="ownerNode"/> and the target nodes.</param>
        void Refresh(IGraphNode ownerNode, NodeContainer nodeContainer);
    }
}
