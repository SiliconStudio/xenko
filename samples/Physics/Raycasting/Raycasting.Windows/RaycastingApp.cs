
using SiliconStudio.Xenko.Engine;

namespace Raycasting
{
    class RaycastingApp
    {
        static void Main(string[] args)
        {
            // Profiler.EnableAll();
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
