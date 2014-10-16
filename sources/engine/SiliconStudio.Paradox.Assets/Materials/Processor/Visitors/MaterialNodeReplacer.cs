// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
namespace SiliconStudio.Paradox.Assets.Materials.Processor.Visitors
{
    public class MaterialNodeReplacer : MaterialBaseVisitor
    {
        public MaterialNodeReplacer(MaterialDescription mat) : base(mat)
        {
        }

        /// <summary>
        /// Replace all occurences of a node in a tree.
        /// </summary>
        public void Replace(IMaterialNode nodeToReplace, IMaterialNode replacementNode)
        {
            Material.VisitNodes((context, nodeEntry) =>
            {
                if (ReferenceEquals(nodeEntry.Node, nodeToReplace))
                {
                    nodeEntry.Node = replacementNode;
                }
            });
        }
    }
}
