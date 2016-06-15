using System.Collections.Generic;
using SiliconStudio.Xenko.Engine;

namespace SpaceEscape.Background
{
    public class BackgroundInfo : ScriptComponent
    {
        public int MaxNbObstacles { get; set; }
        public List<Hole> Holes { get; private set; } = new List<Hole>();
    }
}
