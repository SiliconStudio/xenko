using SiliconStudio.Xenko.Engine;

namespace SimpleParticles
{
    class SimpleParticlesApp
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
