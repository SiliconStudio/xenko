using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Assets.Curves
{
    [DataContract]
    public class CurvePoint
    {
        public ControlPoint ControlPoint1;
        public ControlPoint ControlPoint2;
        public Vector2 Position;
    }
}
