using SiliconStudio.Xenko.Engine;

namespace NativeLinking
{
    class NativeLinkingApp
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
