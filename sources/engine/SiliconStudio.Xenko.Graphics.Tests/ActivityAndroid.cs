using System.Runtime.CompilerServices;
using Android.App;
using Android.OS;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Starter;

namespace SiliconStudio.Paradox.Graphics.Tests
{
    [Activity(Label = "Paradox Graphics", MainLauncher = true, Icon = "@drawable/icon")]
    public class ActivityAndroid : AndroidParadoxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            RuntimeHelpers.RunModuleConstructor(typeof(MaterialKeys).Module.ModuleHandle);

            base.OnCreate(bundle);
            
            //Game = new TestDrawQuad();
            //Game = new TestGeometricPrimitives();
            //Game = new TestRenderToTexture();
            //Game = new TestSpriteBatch();
            //Game = new TestImageLoad();
            //Game = new TestStaticSpriteFont();
            //Game = new TestDynamicSpriteFont();
            //Game = new TestDynamicSpriteFontJapanese();
            Game = new TestDynamicSpriteFontVarious();

            Game.Run(GameContext);
        }
    }
}