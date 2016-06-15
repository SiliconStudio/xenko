
using SiliconStudio.Xenko.Engine;

namespace GravitySensor
{
    class AccelerometerGravityApp
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
