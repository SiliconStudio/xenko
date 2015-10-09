using System.Runtime.CompilerServices;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Starter;

namespace SiliconStudio.Paradox.Input.Tests
{
    public class Application
    {
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }
    }

    [Register("AppDelegate")]
    public class AppDelegate : ParadoxApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            RuntimeHelpers.RunModuleConstructor(typeof(MaterialKeys).Module.ModuleHandle);

            Game = new InputTestGame2();

            return base.FinishedLaunching(app, options);
        }
    }
}