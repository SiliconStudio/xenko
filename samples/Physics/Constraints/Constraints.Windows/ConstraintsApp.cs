
using SiliconStudio.Xenko.Engine;

namespace Constraints
{
    class ConstraintsApp
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
