using Android.App;
using Android.OS;
using SiliconStudio.Xenko.Starter;

namespace SiliconStudio.Xenko.Physics.Tests
{
    [Activity(Label = "Xenko Physics", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : AndroidXenkoActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            //Game = new InputTestGame2();
            //Game.Run(GameContext);
        }
    }
}

