using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.SpriteStudio.Runtime
{
    [DataContract]
    [DataSerializerGlobal(typeof(ReferenceSerializer<SpriteStudioSheet>), Profile = "Asset")]
    [ContentSerializer(typeof(DataContentSerializer<SpriteStudioSheet>))]
    public class SpriteStudioSheet
    {
        public List<SpriteStudioNode> NodesInfo { get; set; }

        public SpriteSheet SpriteSheet { get; set; }
    }
}