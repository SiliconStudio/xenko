// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Linq;

namespace SiliconStudio.Paradox.Assets.Materials.Processor.Visitors
{
    public class MaterialBaseVisitor
    {
        /// <summary>
        /// The material to process.
        /// </summary>
        protected MaterialDescription Material { get; private set; }

        public MaterialBaseVisitor(MaterialDescription mat)
        {
            if (mat == null)
                throw new ArgumentNullException();
            Material = mat;
        }
    }

    public static class MaterialExtensions
    {
        public static void VisitNodes(this MaterialDescription material, Action<object, MaterialNodeEntry> callback, object context = null)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            var nodes = material.Nodes.ToList();
            foreach (var nodeIt in nodes)
            {
                var key = nodeIt.Key;
                VisitNode(new MaterialNodeEntry(nodeIt.Value, node => material.Nodes[key] = node), callback, context);
            }
        }

        public static void VisitNodes(this IMaterialNode node, Action<object, MaterialNodeEntry> callback, object context = null)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            VisitNode(new MaterialNodeEntry(node, DoNothing), callback, context);
        }

        public static void VisitNode(this MaterialNodeEntry nodeEntry, Action<object, MaterialNodeEntry> callback, object context = null)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            callback(context, nodeEntry);
            if (nodeEntry.Node != null)
            {
                foreach (var entry in nodeEntry.Node.GetChildren(context))
                {
                    VisitNode(entry, callback, context);
                }
            }
        }

        private static void DoNothing(IMaterialNode node)
        {
        }
    }
}