using MonoTouch.Foundation;
using MonoTouch.UIKit;
using SiliconStudio.Xenko.Starter;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    public class ManualApplication
    {
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "ManualAppDelegate");
        }
    }

    [Register("ManualAppDelegate")]
    public class ManualAppDelegate : XenkoApplicationDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            //Game = new TestImageLoad();
            //Game = new TestStaticSpriteFont();
            //Game = new TestDynamicSpriteFont();
            //Game = new TestDynamicSpriteFontJapanese();
            Game = new TestDynamicSpriteFontVarious();

            return base.FinishedLaunching(app, options);
        }
    }
}