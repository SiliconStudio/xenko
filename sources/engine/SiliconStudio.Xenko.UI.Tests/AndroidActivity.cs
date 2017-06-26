// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using Android.App;
using Android.OS;
using SiliconStudio.Xenko.Starter;
using SiliconStudio.Xenko.UI.Tests.Rendering;

namespace SiliconStudio.Xenko.UI.Tests
{
    [Activity(Label = "Xenko UI", MainLauncher = true, Icon = "@drawable/icon")]
    public class AndroidActivity : AndroidXenkoActivity
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

