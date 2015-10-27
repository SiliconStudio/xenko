using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace SiliconStudio.Xenko.Audio.Tests
{
    [Register("AppDelegateiOS")]
    public class AppDelegate : UIApplicationDelegate
    {
        UIWindow window;
        MainViewController viewController;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            window = new UIWindow(UIScreen.MainScreen.Bounds);

            viewController = new MainViewController();
            window.RootViewController = viewController;

            window.MakeKeyAndVisible();

            return true;
        }
    }
}

