// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Runtime.CompilerServices;
using Android.App;
using Android.OS;
using SiliconStudio.Xenko.Effects;
using SiliconStudio.Xenko.Starter;

namespace SiliconStudio.Xenko.Graphics.Tests
{
    [Activity(Label = "Xenko Graphics", MainLauncher = true, Icon = "@drawable/icon")]
    public class ActivityAndroid : AndroidXenkoActivity
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
