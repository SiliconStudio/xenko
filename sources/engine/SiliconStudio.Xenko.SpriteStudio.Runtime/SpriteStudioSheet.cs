using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.SpriteStudio.Runtime
{
    [DataContract]
    [DataSerializerGlobal(typeof(ReferenceSerializer<SpriteStudioSheet>), Profile = "Content")]
    [ContentSerializer(typeof(DataContentSerializer<SpriteStudioSheet>))]
    public class SpriteStudioSheet
    {
        public List<SpriteStudioNode> NodesInfo { get; set; }

        public SpriteSheet SpriteSheet { get; set; }

        [DataMemberIgnore]
        public Sprite[] Sprites { get; internal set; }
    }
}