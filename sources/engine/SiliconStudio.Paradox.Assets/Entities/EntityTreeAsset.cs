// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Design;

namespace SiliconStudio.Paradox.Assets.Entities
{
    /// <summary>
    /// Temporary representation of <see cref="EntityHierarchyData"/> used during diff/merge, to map reimported node mapping easily.
    /// </summary>
    [DataContract("EntityTree")]
    class EntityTreeAsset : Asset
    {
        [DataMember(10)]
        public EntityDiffNode Root { get; set; }

        public EntityTreeAsset(EntityHierarchyData hierarchy)
        {
            Root = new EntityDiffNode(hierarchy);
        }

        public EntityTreeAsset()
        {
        }
    }
}