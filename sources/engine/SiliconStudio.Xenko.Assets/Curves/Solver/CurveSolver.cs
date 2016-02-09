using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Assets.Curves.Solver
{
    public static class CurveSolver
    {
        public static IEnumerable<Vector2> Linear(CurveData curve)
        {
            return curve?.Points.Select(point => point.Position);
        }
    }
}
