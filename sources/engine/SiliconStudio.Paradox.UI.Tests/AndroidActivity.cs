using Android.App;
using Android.OS;
using SiliconStudio.Paradox.Starter;
using SiliconStudio.Paradox.UI.Tests.Rendering;

namespace SiliconStudio.Paradox.UI.Tests
{
    [Activity(Label = "Paradox UI", MainLauncher = true, Icon = "@drawable/icon")]
    public class AndroidActivity : AndroidParadoxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            //Program.Main();
            //return;
            
            //Game = new ComplexLayoutRenderingTest();
            //Game = new RenderBorderImageTest();
            //Game = new RenderButtonTest();
            //Game = new RenderImageTest();
            //Game = new RenderTextBlockTest();
            //Game = new RenderEditTextTest();
            //Game = new SeparateAlphaTest();
            //Game = new RenderScrollViewerTest();
            Game = new RenderStackPanelTest();
            Game.Run(GameContext);
        }


    }
}

