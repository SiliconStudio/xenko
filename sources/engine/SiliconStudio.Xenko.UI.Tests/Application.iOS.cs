
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using SiliconStudio.Xenko.Starter;
using SiliconStudio.Xenko.UI.Tests.Rendering;

namespace SiliconStudio.Xenko.UI.Tests
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
            //Game = new RenderEditTextTest();
            //Game = new RenderScrollViewerTest();
            //Game = new ComplexLayoutRenderingTest();
            //Game = new RenderScrollViewerTest();
            Game = new RenderStackPanelTest();

            return base.FinishedLaunching(app, options);
        }
    }
}