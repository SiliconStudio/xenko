
using SiliconStudio.Xenko.Engine;

namespace CharacterControllerSample
{
    class CharacterControllerSampleApp
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
