using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Paradox.SpriteStudio.Offline
{
    [DataContract]
    public class NodeAnimationData
    {
        public NodeAnimationData()
        {
            Data = new Dictionary<string, List<Dictionary<string, string>>>();
        }

        public Dictionary<string, List<Dictionary<string, string>>> Data;
    }
}