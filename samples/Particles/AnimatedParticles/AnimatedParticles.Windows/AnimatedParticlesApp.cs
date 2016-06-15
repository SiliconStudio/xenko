using SiliconStudio.Xenko.Engine;

namespace AnimatedParticles
{
    class AnimatedParticlesApp
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
