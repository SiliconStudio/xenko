
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using SiliconStudio.Paradox.Starter;
using SiliconStudio.Paradox.UI.Tests.Rendering;

namespace SiliconStudio.Paradox.UI.Tests
{
    public class ManualApplication
    {
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "ManualAppDelegate");
        }
    }

    [Register("ManualAppDelegate")]
    public class ManualAppDelegate : ParadoxApplicationDelegate
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