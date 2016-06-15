
using SiliconStudio.Xenko.Engine;

namespace ForwardLighting
{
    class ForwardLightingApp
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
