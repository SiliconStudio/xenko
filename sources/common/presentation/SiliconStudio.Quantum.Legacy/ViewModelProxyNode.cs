// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

using SiliconStudio.Quantum.Legacy.Contents;

namespace SiliconStudio.Quantum.Legacy
{
    /// <summary>
    /// A ViewModelNode that is just a proxy to another node.
    /// </summary>
    public class ViewModelProxyNode : ViewModelNode
    {
        public IViewModelNode TargetNode { get { return targetNode; } set { targetNode = value; targetGuid = value != null ? targetNode.Guid : Guid.Empty; } }

        public Guid TargetGuid
        {
            get { return TargetNode != null ? TargetNode.Guid : targetGuid; }
            set { if (TargetNode == null) targetGuid = value; else throw new InvalidOperationException("Cannot assign a TargetGuid when a TargetNode is already set."); }
        }

        private IViewModelNode targetNode;
        private Guid targetGuid = Guid.Empty;

        public ViewModelProxyNode(string name, IViewModelNode targetNode)
        {
            if (name == null) throw new ArgumentNullException("name");
            Name = name;
            TargetNode = targetNode;
        }

        /// <inheritdoc/>
        public override IContent Content
        {
            get { return TargetNode.Content; }
            set { throw new InvalidOperationException("Cannot assign a Content through a ViewModelProxyNode."); }
        }
    }
}
