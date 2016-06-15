using SiliconStudio.Xenko.Engine;

namespace GameMenu
{
    class GameMenuApp
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
