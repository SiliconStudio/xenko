using SiliconStudio.Xenko.Engine;

namespace CustomParticles
{
    class CustomParticlesApp
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
