using SiliconStudio.Xenko.Engine;

namespace LightningsAndLasers
{
    class LightningsAndLasersApp
    {
        static void Main(string[] args)
        {
            using (var game = new Game())
            {
                game.Run();
            }
        }
    }
}
