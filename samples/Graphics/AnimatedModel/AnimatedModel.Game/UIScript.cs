// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Graphics;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Panels;

namespace AnimatedModel
{
    public class UIScript : StartupScript
    {
        public Entity Knight;

        public SpriteFont Font;

        public override void Start()
        {
            base.Start();

            // Bind the buttons
            var page = Entity.Get<UIComponent>().Page;

            var btnIdle = page.RootElement.FindVisualChildOfType<Button>("ButtonIdle");
            btnIdle.Click += (s, e) => Knight.Get<AnimationComponent>().Crossfade("Idle", TimeSpan.FromSeconds(0.25));

            var btnRun = page.RootElement.FindVisualChildOfType<Button>("ButtonRun");
            btnRun.Click += (s, e) => Knight.Get<AnimationComponent>().Crossfade("Run", TimeSpan.FromSeconds(0.25));

            // Set the default animation
            Knight.Get<AnimationComponent>().Play("Run");
        }        
    }
}
