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