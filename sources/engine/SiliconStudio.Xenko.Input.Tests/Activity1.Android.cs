using Android.App;
using Android.OS;
using SiliconStudio.Paradox.Starter;

namespace SiliconStudio.Paradox.Input.Tests
{
    [Activity(Label = "Paradox Input", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : AndroidParadoxActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Game = new InputTestGame2();
            Game.Run(GameContext);
        }
    }
}

