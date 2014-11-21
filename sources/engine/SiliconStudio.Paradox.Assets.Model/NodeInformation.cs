// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Assets.Model
{
    [DataContract("NodeInformation")]
    [DataStyle(DataStyle.Compact)]
    public class NodeInformation : IDiffKey
    {
        /// <summary>
        /// The name of the node.
        /// </summary>
        [DataMember(10), DiffUseAsset2]
        public string Name;

        /// <summary>
        ///  The index of the parent.
        /// </summary>
        [DataMember(20), DiffUseAsset2]
        public int Depth;

        /// <summary>
        /// A flag stating if the node is collapsable.
        /// </summary>
        [DataMember(30)]
        [DefaultValue(true)]
        public bool Preserve;

        public NodeInformation()
        {
            Preserve = true;
        }

        public NodeInformation(string name, int depth, bool preserve)
        {
            Name = name;
            Depth = depth;
            Preserve = preserve;
        }

        object IDiffKey.GetDiffKey()
        {
            return Name;
        }
    }
}