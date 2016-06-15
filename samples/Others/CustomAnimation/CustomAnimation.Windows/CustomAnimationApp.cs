using SiliconStudio.Xenko.Engine;

namespace CustomAnimation
{
    class CustomAnimationApp
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
