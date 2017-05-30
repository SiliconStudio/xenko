// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
namespace SiliconStudio.Quantum
{
    /// <summary>
    /// An interface representing an <see cref="IGraphNode"/> during its initialization phase.
    /// </summary>
    public interface IInitializingGraphNode : IGraphNode
    {
        /// <summary>
        /// Seal the node, indicating its construction is finished and that no more children will be added.
        /// </summary>
        void Seal();
    }
}
