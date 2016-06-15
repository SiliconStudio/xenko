
using SiliconStudio.Xenko.Engine;

namespace MaterialSample
{
    class MaterialSampleApp
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
